using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FireworksFramework.Mqtt;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NModbus;
using NModbus.Utility;
using PeiuPlatform.App;
using PeiuPlatform.Lib;

namespace PeiuPlatform.Hubbub
{
    public class ModbusReadWriteWorker : BackgroundService
    {
        private readonly ILogger<ModbusReadWriteWorker> _logger;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly IGlobalStorage<EventModel> globalStorage;
        private readonly HubbubInformation hubbubInformation;
        TcpClient client = null;

        public ModbusReadWriteWorker(ILogger<ModbusReadWriteWorker> logger,
            HubbubInformation hubbubInformation,
            IHostApplicationLifetime hostApplicationLifetime,
            IGlobalStorage<EventModel> globalStorage)
        {
            _logger = logger;
            this.hubbubInformation = hubbubInformation;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.globalStorage = globalStorage;

            //AbsMqttBase.SetDefaultLoggerName("nlog.config", true);
            //EventPublisherWorker worker = new EventPublisherWorker(1);
            //worker.Initialize();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //CancellationToken cancelToken = hostApplicationLifetime.ApplicationStopping;
            IEnumerable<DataPoint> pointList = null;

            // Step 1. Reading modbusmap
            try
            {
                using (MapReader reader = new MapReader("modbusmap.csv"))
                    pointList = reader.ReadToEnd();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            var pointGroup = pointList.GroupBy(x => x.Category);
            
            // Step 2. Initialize modbus
            var modbusFactory = new ModbusFactory();
            IModbusMaster master = null;
            //report = new StreamWriter("datareport.txt", false, Encoding.UTF8);
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
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

                    var writeCommandList = await globalStorage.GetWriteValues(stoppingToken);
                    foreach(ModbusWriteCommand command in writeCommandList)
                    {
                        switch(command.FunctionCode)
                        {
                            case 3:
                                await master.WriteSingleRegisterAsync(1, command.StartAddress, command.WriteValue);
                                _logger.LogInformation($"Write Modbus: {command.StartAddress} : {command.WriteValue}");

                                if(command.StartAddress == 499 ||
                                    command.StartAddress == 605 ||
                                    command.StartAddress == 805 ||
                                    command.StartAddress == 1005 ||
                                    command.StartAddress == 1205) 
                                {
                                    // 만약 Local 에서 Remote로 변경할 경우 2초후에 ActivePower값을 전부 0으로 준다
                                    await Task.Delay(TimeSpan.FromSeconds(2));
                                    await master.WriteSingleRegisterAsync(1, 601, 0);
                                    await master.WriteSingleRegisterAsync(1, 801, 0);
                                    await master.WriteSingleRegisterAsync(1, 1001, 0);
                                    await master.WriteSingleRegisterAsync(1, 1201, 0);
                                }
                                break;
                        }
                    }

                    // Step 4. Read holding register
                    foreach (var row in pointGroup)
                    {
                        await ReadPointAsync(master, row, stoppingToken);
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
                finally
                {
                    //if (report != null)
                    //{
                    //    report.Close();
                    //    report = null;
                    //}
                    //StopReporting = true;
                }
                await Task.Delay(hubbubInformation.ScanRateMS, stoppingToken);
            }
        }

        private EventModel CreateEventModel(int DeviceType, int DeviceIndex, int GroupCode, ushort Value)
        {
            EventModel record = new EventModel();
            record.SetTimestamp(DateTime.Now);
            //record.UnixTimestamp = DateTimeOffset.Now.ToUniversalTime().ToUnixTimeSeconds();
            record.DeviceType = DeviceType;
            record.DeviceIndex = DeviceIndex;
            record.SiteId = 6;
            record.Status = EventStatus.New;
            record.GroupCode = GroupCode;
            record.BitFlag = Value;
            //record.FactoryCode = 1;
            return record;
        }

        private void AlarmCheck(string category, string pointname, ushort value)
        {
            EventModel model = null;
            if(category.StartsWith("PCS#") && pointname.StartsWith("Alarm"))
            {
                int deviceidx = int.Parse(category.TrimStart("PCS#".ToCharArray()));
                if(pointname == "Alarm1")
                {
                    model = CreateEventModel(0, deviceidx, 500, value);
                }
                else if(pointname == "Alarm2")
                {
                    model = CreateEventModel(0, deviceidx, 501, value);
                }
                
            }
            else if (category.StartsWith("BSC#") && pointname.StartsWith("Event"))
            {
                int deviceidx = int.Parse(category.TrimStart("BSC#".ToCharArray()));
                if (pointname == "Event1")
                {
                    model = CreateEventModel(1, deviceidx, 600, value);
                }
                else if (pointname == "Event2")
                {
                    model = CreateEventModel(1, deviceidx, 601, value);
                }
                else if (pointname == "Event3")
                {
                    model = CreateEventModel(1, deviceidx, 602, value);
                }
                else if (pointname == "Event4")
                {
                    model = CreateEventModel(1, deviceidx, 603, value);
                }

            }

            if (model != null)
                globalStorage.SetEventValues(model);
        }
        private async Task ReadPointAsync(IModbusMaster master, IGrouping<string, DataPoint> row, CancellationToken cancellationToken)
        {
            string category = row.Key;
            ushort startAddress = row.Min(x => x.Address);
            ushort lastOffset = row.Max(x => x.Address);
            ushort numOfPoints = (ushort)(row.FirstOrDefault(x => x.Address == lastOffset).WordSize + (lastOffset - startAddress));
            ushort[] dataOfPoints = await master.ReadHoldingRegistersAsync(1, startAddress, numOfPoints);

            foreach(DataPoint point in row)
            {
                try
                {
                    int offset = point.Address - startAddress;

                    ushort[] readData = new ushort[point.WordSize];
                    Array.Copy(dataOfPoints, offset, readData, 0, point.WordSize);

                    IComparable readValue = ParseReadData(readData, point.Type);
                    if (point.Name == "OperationStatus")
                    {
                        UpdateStatus(point.Category, readValue);
                    }
                    else if(point.Name == "ManualAuto")
                    {
                        globalStorage.SetValue($"{category}.Stat.{Model.PcsStatus.MANUALAUTO}", readValue);
                    }

                    if(point.Name == "LocalRemote")
                    {
                        globalStorage.SetValue($"{category}.LocalRemote", (ushort)readValue == 3 ? 1 : 0);
                        continue;
                    }

                    if (point.IsDigit)
                    {
                        readValue = ReadDigit(readValue, point.Digit);
                    }
                    if (point.Scale != 0)
                        readValue = (dynamic)readValue * point.Scale;

#if EVENT
                    if (point.Name.StartsWith("Alarm") || point.Name.StartsWith("Event"))
                    {
                        AlarmCheck(row.Key, point.Name, Convert.ToUInt16(readValue));
                    }
#endif

                    globalStorage.SetValue(point.GetUniqueId(), readValue);
                    //if (StopReporting == false)
                    //    await report.WriteLineAsync($"{point.Category},{point.Name},{point.Address},{readValue}");
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"[{point.GetUniqueId()}] {ex.Message}");
                }
                
            }
        }

        private byte ReadDigit(IComparable value, int digitNum)
        {
            byte[] buffers = BitConverter.GetBytes((dynamic)value);
            return buffers[digitNum];
        }

        private void UpdateStatus(string category, IComparable value)
        {
            int run = BitWise(value, PcsOperationStatus.RunStop);
            int stop = run == 1 ? 0 : 1;
            int standby = BitWise(value, PcsOperationStatus.StandBy);
            int fault = BitWise(value, PcsOperationStatus.Fault);
            int charge = BitWise(value, PcsOperationStatus.Charge);
            int discharge = BitWise(value, PcsOperationStatus.Discharge);
            int ac_cb = BitWise(value, PcsOperationStatus.AC_CB);
            int warning = BitWise(value, PcsOperationStatus.Warning);
            int ac_mc = BitWise(value, PcsOperationStatus.AC_MC);
            int dc_cb = BitWise(value, PcsOperationStatus.DC_CB);

            globalStorage.SetValue($"{category}.Stat.{Model.PcsStatus.RUN}", run);
            globalStorage.SetValue($"{category}.Stat.{Model.PcsStatus.STOP}", stop);
            globalStorage.SetValue($"{category}.Stat.{Model.PcsStatus.STAND_BY}", standby);
            globalStorage.SetValue($"{category}.Stat.{Model.PcsStatus.CHARGE}", charge);
            globalStorage.SetValue($"{category}.Stat.{Model.PcsStatus.DISCHARGE}", discharge);
            globalStorage.SetValue($"{category}.Stat.{Model.PcsStatus.EMERGENCY}", 0);
            globalStorage.SetValue($"{category}.Stat.{Model.PcsStatus.AC_CB}", ac_cb);
            globalStorage.SetValue($"{category}.Stat.{Model.PcsStatus.AC_MC}", ac_mc);
            globalStorage.SetValue($"{category}.Stat.{Model.PcsStatus.DC_CB}", dc_cb);
            globalStorage.SetValue($"{category}.Stat.{Model.PcsStatus.FAULT}", fault);
            globalStorage.SetValue($"{category}.Stat.{Model.PcsStatus.WARNING}", warning);
            
            
        }

        private int BitWise(IComparable value, ushort BitFlag)
        {
            bool result = (((dynamic)value & BitFlag) == BitFlag);
            return result ? 1 : 0;
        }

        private IComparable ParseReadData(ushort[] readData, DataType type)
        {
            switch (type)
            {
                case DataType.I16:
                    Int16 i16Value = (short)readData[0];
                    return i16Value;
                case DataType.I32:
                    Int32 i32Value = (Int32)ModbusUtility.GetUInt32(readData[0], readData[1]);
                    return i32Value;
                case DataType.U32:
                    UInt32 ui32Value = ModbusUtility.GetUInt32(readData[0], readData[1]);
                    return ui32Value;
                case DataType.SGL:
                    float fValue = ModbusUtility.GetSingle(readData[1], readData[0]);
                    return fValue;
                default: // U16
                    ushort u16Value = readData[0];
                    return u16Value;
            }
        }
        

        private async Task<TcpClient> TryConnecting(CancellationToken stoppingToken)
        {

            while (stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    TcpClient client = new TcpClient(this.hubbubInformation.ModbusSlaveIp, this.hubbubInformation.ModbusSlavePort);
                    _logger.LogWarning("### CONNECTED WITH MODBUS SERVER ###");
                    globalStorage.SetValue($"PCS#1.Stat.COMM", 0);
                    globalStorage.SetValue($"PCS#2.Stat.COMM", 0);
                    globalStorage.SetValue($"PCS#3.Stat.COMM", 0);
                    globalStorage.SetValue($"PCS#4.Stat.COMM", 0);
                    return client;
                }
                catch (Exception ex)
                {
                    globalStorage.SetValue($"PCS#1.Stat.COMM", 1);
                    globalStorage.SetValue($"PCS#2.Stat.COMM", 1);
                    globalStorage.SetValue($"PCS#3.Stat.COMM", 1);
                    globalStorage.SetValue($"PCS#4.Stat.COMM", 1);
                    _logger.LogError("### RECONNECTING TO MODBUS SLAVE FAILED ###");
                    _logger.LogInformation("TRY CONNECTING AFTER 5 SECONDS");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
            return null;
        }
    }
}
