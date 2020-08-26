using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NModbus;
using StackExchange.Redis;

namespace PeiuPlatform.Hubbub
{
     internal partial class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IRedisStorage _dataAccessor;
        private readonly IDatabase _db;
        private readonly MysqlDataAccess _mysqlDa;
        private readonly int intervalms = 60000;
        private readonly HubbubInformation hubbubInformation;
        TcpClient client = null;
        

        public Worker(ILogger<Worker> logger, IRedisStorage dataAccessor, 
            IConfiguration configuration, HubbubInformation hubbubInformation,
            ConnectionMultiplexer connectionMultiplexer, MysqlDataAccess mysqlDataAccess)
        {
            _logger = logger;
            _dataAccessor = dataAccessor;
            _db = connectionMultiplexer.GetDatabase(2);
            _mysqlDa = mysqlDataAccess;
            intervalms = configuration.GetSection("IntervalMS").Get<int>();
            this.hubbubInformation = hubbubInformation;
        }


        private async Task<TcpClient> TryConnecting(CancellationToken stoppingToken)
        {

            while (stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    TcpClient client = new TcpClient(this.hubbubInformation.ModbusSlaveIp, this.hubbubInformation.ModbusSlavePort);
                    _logger.LogWarning("### CONNECTED WITH MODBUS SERVER ###");
                   
                    return client;
                }
                catch (Exception ex)
                {
                    _logger.LogError("### RECONNECTING TO MODBUS SLAVE FAILED ###");
                    _logger.LogInformation("TRY CONNECTING AFTER 5 SECONDS");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
            return null;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            var modbusFactory = new ModbusFactory();
            IModbusMaster master = null;
            DateTime readTimeTrigger = DateTime.MinValue;
            while (!stoppingToken.IsCancellationRequested)
            {
                try

                {
                    // Step 3. Connect to modbus
                    if (client == null)
                    {
                        client = await TryConnecting(stoppingToken);
                        if (client == null)
                        {
                            _logger.LogWarning("## MODBUS Monitor worker is break");
                            return;
                        }
                        master = modbusFactory.CreateMaster(client);
                    }

                    //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    using (var session = _mysqlDa.SessionFactory.OpenSession())
                    using (var trans = session.BeginTransaction())
                    {

                        RunCommand(session, master, stoppingToken);

                        if (readTimeTrigger <= DateTime.Now)
                        {
                            TbStatus stat = await ReadValues(stoppingToken);
                            if (stat != null)
                            {
                                await session.SaveOrUpdateAsync(stat, stoppingToken);
                                await trans.CommitAsync(stoppingToken);
                                _logger.LogInformation("Save at: {time}", DateTimeOffset.Now);
                            }
                            readTimeTrigger = DateTime.Now.AddMilliseconds(intervalms);
                        }
                    }
                }
                catch (IOException ioex)
                {
                    if (ioex.Source == "System.Net.Sockets")
                    {
                        client.Dispose();
                        client = null;
                    }
                    _logger.LogError(ioex, ioex.Message);
                    await Task.Delay(hubbubInformation.TryConnectTimeMS, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }

                await Task.Delay(100, stoppingToken);
            }
        }

        private  string GetStat(int stat)
        {
            if ((stat & 2) == 2)
            {
                // Running
                if ((stat & 32) == 32)
                {
                    return "충전중";
                }
                else if ((stat & 64) == 64)
                {
                    return "방전중";
                }
                else
                {
                    return "대기";
                }
            }
            else
            {
                return "정지";
            }
        }

        private async Task<TbStatus> ReadValues(CancellationToken stoppingToken)
        {
            try

            {
                float pv_freq;
                float pv_act = await _dataAccessor.GetValue<float>("GIPAM#1.ActivePower");
                float pv_rctPwr;
                float pv_appPwr;

                pv_freq = (float)await _dataAccessor.GetValue<float>("GIPAM#1.Frequency");
                pv_rctPwr = (float)await _dataAccessor.GetValue<float>("GIPAM#1.ReactivePower");
                pv_appPwr = (float)await _dataAccessor.GetValue<float>("GIPAM#1.ApparentPower");

                float pv_act_forward_high = (float)await _dataAccessor.GetValue<float>("GIPAM#1.TotalForwardActiveEnergyHigh");
                float pv_act_forward_low = (float)await _dataAccessor.GetValue<float>("GIPAM#1.TotalForwardActiveEnergyLow");
                float pv_act_reverse_high = (float)await _dataAccessor.GetValue<float>("GIPAM#1.TotalReverseActiveEnergyHigh");
                float pv_act_reverse_low = (float)await _dataAccessor.GetValue<float>("GIPAM#1.TotalReverseActiveEnergyLow");

                float pcs_freq = (float)await _dataAccessor.GetValue<float>("GIPAM#2.Frequency");
                float pcs_act = (float)await _dataAccessor.GetValue<float>("GIPAM#2.ActivePower");
                float pcs_rctPwr = (float)await _dataAccessor.GetValue<float>("GIPAM#2.ReactivePower");
                float pcs_appPwr = (float)await _dataAccessor.GetValue<float>("GIPAM#2.ApparentPower");
                float pcs_act_forward_high = (float)await _dataAccessor.GetValue<float>("GIPAM#2.TotalForwardActiveEnergyHigh");
                float pcs_act_forward_low = (float)await _dataAccessor.GetValue<float>("GIPAM#2.TotalForwardActiveEnergyLow");
                float pcs_act_reverse_high = (float)await _dataAccessor.GetValue<float>("GIPAM#2.TotalReverseActiveEnergyHigh");
                float pcs_act_reverse_low = (float)await _dataAccessor.GetValue<float>("GIPAM#2.TotalReverseActiveEnergyLow");

                float pv_accum = pv_act_forward_high;
                float pcs_chg_Accum = pcs_act_reverse_high;
                float pcs_dhg_Accum = pcs_act_forward_high;

                TbStatus status = new TbStatus();
                status.Date = DateTime.Now;
                status.Pv = pv_act;
                status.PvEng = pv_accum / 1000;
                status.Ess = pcs_act;
                status.EssCh = pcs_chg_Accum / 1000;
                status.EssDch = pcs_dhg_Accum / 1000;
                float[] socs = new float[4];
                float[] acts = new float[4];
                float[] pv_today_eng = new float[4];
                int[] pcsstatus = new int[4];
                for (int i = 1; i < 5; i++)
                {
                    float soc1 = await _dataAccessor.GetValue<float>($"BSC#{i}.SOC");
                    float act = (float)await _dataAccessor.GetValue<float>($"PCS#{i}.ActivePower");
                    pcsstatus[i - 1] = (int)await _dataAccessor.GetValue<int>($"PCS#{i}.OperationStatus");

                    socs[i - 1] = soc1;
                    acts[i - 1] = act;

                }
                status.Soc1 = socs[0];
                status.Soc2 = socs[1];
                status.Soc3 = socs[2];
                status.Soc4 = socs[3];
                status.Stat1 = GetStat(pcsstatus[0]);
                status.Stat2 = GetStat(pcsstatus[1]);
                status.Stat3 = GetStat(pcsstatus[2]);
                status.Stat4 = GetStat(pcsstatus[3]);
                status.Pcs1ActPwr = acts[0];
                status.Pcs2ActPwr = acts[1];
                status.Pcs3ActPwr = acts[2];
                status.Pcs4ActPwr = acts[3];
                return status;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }
    }
}
