using PEIU.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PES.Toolkit;
using PES.Toolkit.Config;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PES.Service.DataService
{
    public class MongoBackgroundHostService : BackgroundService
    {
        private readonly ILogger _logger;
        public IBackgroundMongoTaskQueue TaskQueue { get; }
        private readonly MqttSubscribeConfig _mqttConfig;
        readonly MysqlDataAccess _da;
        readonly TimeSpan BatchInterval;
        readonly ManualResetEvent mre = new ManualResetEvent(true);
        private ConcurrentBag<DaegunPacketClass> insertBatchList;
       

        public MongoBackgroundHostService(
            IBackgroundMongoTaskQueue taskQueue,
            IConfiguration configuration, 
            MqttSubscribeConfig mqttConfig,
            ILoggerFactory loggerFactory,
            MysqlDataAccess da
            )
        {

            TaskQueue = taskQueue;
            _mqttConfig = mqttConfig;
            insertBatchList = new ConcurrentBag<DaegunPacketClass>();
            _da = da;
            BatchInterval = configuration.GetSection("BatchInterval").Get<TimeSpan>();
            _logger = loggerFactory.CreateLogger<MongoBackgroundHostService>();
        }
        Dictionary<int, DateTime> lastRecordTime = new Dictionary<int, DateTime>();

        private BmsData CreateBmsData(DaegunPacketClass _packet)
        {
            DaegunPacket packet = _packet.Packet;
            BmsData bmsData = new BmsData();
            bmsData.deviceId = $"DS{packet.sSiteId}_BMS{packet.Bsc.PcsIndex}";
            bmsData.groupId = 2;
            bmsData.siteId = packet.sSiteId;
            bmsData.timestamp = _packet.Timestamp;
            bmsData.groupName = "PCS_BATTERY";
            bmsData.soc = packet.Bsc.Soc;
            bmsData.soh = packet.Bsc.Soh;
            bmsData.dcPwr = packet.Pcs.Dc_battery_power;
            bmsData.dcVlt = packet.Pcs.Dc_battery_voltage;
            bmsData.dcCrt = packet.Pcs.Dc_battery_current;
            bmsData.cellMaxTemp = packet.Bsc.ModuleTempRange.Max;
            bmsData.cellMinTemp = packet.Bsc.ModuleTempRange.Min;
            bmsData.cellMinVlt = packet.Bsc.CellVoltageRange.Min;
            bmsData.cellMaxVlt = packet.Bsc.CellVoltageRange.Max;
            return bmsData;
            
        }

        private PvData CreatePvData(DaegunPacketClass _packet)
        {
            DaegunMeterPacket pv_pack = _packet.Packet.Pv;
            PvData pv = new PvData();
            pv.deviceId = $"DS{_packet.Packet.sSiteId}_PV{pv_pack.DeviceIndex}";
            pv.groupId = 3;
            pv.siteId = _packet.Packet.sSiteId;
            pv.groupName = "PV_SYSTEM";
            pv.TotalActivePower = pv_pack.TotalActivePower;
            pv.TotalReactivePower = pv_pack.TotalReactivePower;
            pv.ReverseActivePower = pv_pack.ReverseActivePower;
            pv.ReverseReactivePower = pv_pack.ReverseReactivePower;
            pv.vltR = pv_pack.Voltage.R;
            pv.vltS = pv_pack.Voltage.S;
            pv.vltT = pv_pack.Voltage.T;
            pv.crtR = pv_pack.Current.R;
            pv.crtS = pv_pack.Current.S;
            pv.crtT = pv_pack.Current.T;
            pv.Frequency = pv_pack.Frequency;
            pv.EnergyTotalActivePower = pv_pack.EnergyTotalActivePower;
            pv.EnergyTotalReactivePower = pv_pack.EnergyTotalReactivePower;
            pv.EnergyTotalReverseActivePower = pv_pack.EnergyTotalReverseActivePower;
            pv.timestamp = _packet.Timestamp;
            return pv;

        }

        private PcsData CreatePcsData(DaegunPacketClass _packet)
        {
            DaegunPacket packet = _packet.Packet;
            PcsData pcs = new PcsData();
            pcs.deviceId = $"DS{packet.sSiteId}_PCS{packet.Pcs.PcsNumber}";
            pcs.groupId = 1;
            pcs.siteId = packet.sSiteId;
            pcs.groupName = "PCS_SYSTEM";
            pcs.freq = packet.Pcs.Frequency;
            pcs.acVltR = packet.Pcs.AC_line_voltage.R;
            pcs.acVltS = packet.Pcs.AC_line_voltage.S;
            pcs.acVltT = packet.Pcs.AC_line_voltage.T;
            pcs.actPwrCmdLmtLowChg = 0;
            pcs.actPwrCmdLmtLowDhg = 0;
            pcs.actPwrCmdLmtHighChg = packet.Bsc.ChargePowerLimit;
            pcs.actPwrCmdLmtHighDhg = packet.Bsc.DischargePowerLimit;
            pcs.actPwr = packet.Pcs.ActivePower;
            pcs.acCrtLow = 0;
            pcs.acCrtHigh = packet.Bsc.DischargeCurrentLimit;
            pcs.acCrtR = packet.Pcs.AC_phase_current.R;
            pcs.acCrtS = packet.Pcs.AC_phase_current.S;
            pcs.acCrtT = packet.Pcs.AC_phase_current.T;
            pcs.rctPwr = packet.Pcs.ReactivePower;
            pcs.pf = packet.Pcs.PowerFactor;
            pcs.timestamp = _packet.Timestamp;
            return pcs;
        }

        private async void StoreDb(CancellationToken token)
        {
            try
            {
                if (insertBatchList.Count == 0)
                    return;

                List<PcsData> pcsDatas = new List<PcsData>();
                List<BmsData> bmsDatas = new List<BmsData>();
                List<PvData> pvDatas = new List<PvData>();
                foreach(var packet in insertBatchList)
                {
                    PcsData pcs = CreatePcsData(packet);
                    pcsDatas.Add(pcs);

                    bmsDatas.Add(CreateBmsData(packet));
                    pvDatas.Add(CreatePvData(packet));
                }

                using (var session = _da.SessionFactory.OpenStatelessSession())
                {
                    using (var transact = session.BeginTransaction())
                    {
                        foreach (var pcs in pcsDatas)
                            await session.InsertAsync(pcs, token);

                        foreach (var bms in bmsDatas)
                            await session.InsertAsync(bms, token);

                        foreach (var pv in pvDatas)
                            await session.InsertAsync(pv, token);
                        await transact.CommitAsync(token);
                    }
                }

                insertBatchList.Clear();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DateTime batchTime = DateTime.MinValue;
            while (!stoppingToken.IsCancellationRequested)
            {
                //var workItem = await TaskQueue.DequeueAsync(stoppingToken);
                mre.WaitOne();
                DaegunPacketClass queueItem = await TaskQueue.DequeueAsync(stoppingToken);
                DaegunPacket modbusWorkItem = queueItem.Packet;
                try
                {
                    //modbusWorkItem.timestamp = DateTime.Now;
                    
                    //string pcs_key = $"site_{modbusWorkItem.sSiteId}_PCS";
                    //string bsc_key = $"site_{modbusWorkItem.sSiteId}_BSC";
                    //string ess_key = $"site_{modbusWorkItem.sSiteId}_ESSMETER";
                    //string pv_key = $"site_{modbusWorkItem.sSiteId}_PVMETER";
                    //HashEntry lastUpdate = new HashEntry("timestamp", DateTime.Now.ToString());

                    //var hashPcsMap = RedisConnectionFactory.CreateHashEntries(modbusWorkItem.Pcs);
                    //hashPcsMap.Add(lastUpdate);
                    //var hashbmsMap = RedisConnectionFactory.CreateHashEntries(modbusWorkItem.Bsc, lastUpdate);
                    //var hashmeter_ess_Map = RedisConnectionFactory.CreateHashEntries(modbusWorkItem.Ess, lastUpdate);
                    //var hashmeter_pv_Map = RedisConnectionFactory.CreateHashEntries(modbusWorkItem.Pv, lastUpdate);

                    
                    //await _redisDb.HashSetAsync(pcs_key, hashPcsMap.ToArray(), CommandFlags.FireAndForget);
                    //await _redisDb.HashSetAsync(bsc_key, hashbmsMap.ToArray(), CommandFlags.FireAndForget);
                    //await _redisDb.HashSetAsync(ess_key, hashmeter_ess_Map.ToArray(), CommandFlags.FireAndForget);
                    //await _redisDb.HashSetAsync(pv_key, hashmeter_pv_Map.ToArray(), CommandFlags.FireAndForget);

                    if (lastRecordTime.ContainsKey(modbusWorkItem.sSiteId) == false)
                    {
                        lastRecordTime.Add(modbusWorkItem.sSiteId, DateTime.MinValue);
                    }

                    insertBatchList.Add(queueItem);

                    //if (DateTime.Now > lastRecordTime[modbusWorkItem.sSiteId])
                    //{
                    //    insertBatchList.Add(queueItem);

                    //    //_db = _client.GetDatabase("PEIU");
                    //    //cols = _db.GetCollection<DaegunPacket>("daegun_meter");
                    //    ////lastRecord = DateTime.Now.Add(_mqttConfig.RecordInterval);
                    //    //await cols.InsertOneAsync(modbusWorkItem);
                    //    //_logger.LogInformation("Store packet from daegun");
                    //    lastRecordTime[modbusWorkItem.sSiteId] = DateTime.Now.Add(_mqttConfig.RecordInterval);
                    //}

                    if (batchTime <= DateTime.Now)
                    {
                        StoreDb(stoppingToken);
                        batchTime = DateTime.Now.Add(BatchInterval);
                    }
                    

                    //Console.WriteLine($"SAVE DB");
                    //  if(workItem != null)
                    //    await workItem(stoppingToken);
                    //if (modbusWorkItem != null)
                    //    await modbusWorkItem.RunAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                       $"Error occurred executing {nameof(ex)}");
                    Console.WriteLine(ex);
                }


            }

            _logger.LogInformation("Queued Hosted Service is stopping.");
        }
    }
}
