using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NModbus;
using NModbus.Serial;
using NModbus.Utility;
using PeiuPlatform.Model.ExchangeModel;
using PeiuPlatform.Models;
using RestSharp;
using StackExchange.Redis;

namespace Hubbub
{
    public class ModbusAcquisitionService : BackgroundService
    {
        private readonly ILogger<ModbusAcquisitionService> _logger;
        
        
        private readonly ModbusFactory modbusFactory;
        //private readonly ConcurrentDictionary<int, IModbusMaster> modbusMasters = null;
        //private readonly TwoPairKeyHashCube<int, int, IComparable> DIPointHashCube = null;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly HubbubInformation hubbubInformation;

#if USE_REDIS
        private readonly IDatabaseAsync database;
#else
        //private readonly IAsyncDataAccessor<JObject> globalStorage;
        
#endif
        private const int MAX_READ_MODBUS_COUNT = 125;
        private readonly string ZkServerHost;
        //private readonly ThreeKeyHashCube<int, int, string, IComparable> AnalogHashCube;
        //private readonly TwoPairKeyHashCube<int, int, JObject> JsonTemplateHashCube;
        //private readonly TwoPairKeyHashCube<int, int, IComparable> AnalogHashCube;
        private readonly ConcurrentQueue<PushModel> globalQueue;
        private readonly ConcurrentQueue<EventModel> globalEventQueue;
        private readonly ConcurrentQueue<ModbusControlModel> globalControlQueue;
        private ModbusHubbubMappingTemplate HubbubTemaplte;
        //Dictionary<int, ModbusHubbubMappingTemplate> hubbubTemplates = null;

        public ModbusAcquisitionService(ILogger<ModbusAcquisitionService> logger,
            IConfiguration configuration,
            IHostApplicationLifetime hostApplicationLifetime,
            ConcurrentQueue<PushModel> globalQueue,
            ConcurrentQueue<EventModel> globalEventQueue,
            ConcurrentQueue<ModbusControlModel> globalControlQueue,
            ModbusHubbubMappingTemplate HubbubTemaplte
#if USE_REDIS
            ConnectionMultiplexer connectionMultiplexer
#else
            //IAsyncDataAccessor<JObject> globalStorage
#endif
            )
        {
            _logger = logger;
            ZkFactory.Initialize(logger);
            this.hubbubInformation = HubbubInformation.GlobalHubbubInformation;
            this.HubbubTemaplte = HubbubTemaplte;
            ZkServerHost = hubbubInformation.ZookeeperHost;
            this.hostApplicationLifetime = hostApplicationLifetime;
            //hubbubTemplates = new Dictionary<int, ModbusHubbubMappingTemplate>();
            //modbusMasters = new ConcurrentDictionary<int, IModbusMaster>();
            modbusFactory = new ModbusFactory();
            //DIPointHashCube = new TwoPairKeyHashCube<int, int, IComparable>();
            this.globalQueue = globalQueue;
            this.globalEventQueue = globalEventQueue;
            this.globalControlQueue = globalControlQueue;
#if USE_REDIS
            database = connectionMultiplexer.GetDatabase();
#else
            //this.globalStorage = globalStorage;
#endif
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            //await DownloadAnalogTemplateAsync(cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        //private void InitModbusMaster()
        //{
        //    modbusMasters = new Dictionary<int, IModbusMaster>();
        //    var modbusFactory = new ModbusFactory();
        //    foreach (ModbusHubbubTemplate hubbub in AnalogTemplates)
        //    {
        //        if(modbusMasters.ContainsKey(hubbub.ConnectionInfo.Id) == false)
        //        {
        //            IModbusMaster master = CreateModbusMaster(modbusFactory, hubbub.ConnectionInfo);
        //            modbusMasters.Add(hubbub.ConnectionInfo.Id, master);
        //            //IModbusMaster master = 
        //        }
        //    }
        //

        private EventModel CreateEventModel(int DeviceType, int DeviceIndex, int GroupCode, ushort Value)
        {
            EventModel record = new EventModel();
            record.UnixTimestamp = DateTimeOffset.Now.ToUniversalTime().ToUnixTimeSeconds();
            record.DeviceType = DeviceType;
            record.DeviceIndex = DeviceIndex;
            record.SiteId = HubbubTemaplte.Hubbub.Siteid;
            record.Status = EventStatus.New;
            record.GroupCode = GroupCode;
            record.BitFlag = Value;
            record.FactoryCode = 1;
            return record;
        }

        private IModbusMaster CreateModbusMaster(ModbusFactory modbusFactory, ModbusConnectionInfo modbusConnectionInfo)
        {
            IModbusMaster master = null;
            switch(modbusConnectionInfo.Protocolid)
            {
                case ProtocolTypes.MODBUS_TCP_SLAVE:
                    TcpClient tcpClient = new TcpClient(modbusConnectionInfo.Host, modbusConnectionInfo.Port.Value);
                    master = modbusFactory.CreateMaster(tcpClient);
                    break;
                case ProtocolTypes.MODBUS_RTU_SLAVE:
                    {
                        SerialPort serialPort = CreateSerialPortConfigAndOpen(modbusConnectionInfo);
                        //var adapter = new SerialPortAdapter(serialPort);
                        master = modbusFactory.CreateRtuMaster(serialPort);
                    }
                    break;
                case ProtocolTypes.MODBUS_ASCII_SLAVE:
                    {
                        SerialPort serialPort = CreateSerialPortConfigAndOpen(modbusConnectionInfo);
                        //var adapter = new SerialPortAdapter(serialPort);
                        master = modbusFactory.CreateAsciiMaster(serialPort);
                    }
                    break;
                default:
                    throw new NotSupportedException("모드버스 이외의 통신은 현재 지원하지 않습니다");
            }
            _logger.LogInformation($"[Worker] Connected to modbus {modbusConnectionInfo.Host}");
            return master;
        }

        private SerialPort CreateSerialPortConfigAndOpen(ModbusConnectionInfo modbusConnectionInfo)
        {
            SerialPort serialPort = new SerialPort(modbusConnectionInfo.Host);
            serialPort.BaudRate = modbusConnectionInfo.Baudrate;
            serialPort.DataBits = modbusConnectionInfo.Databits;
            serialPort.Parity = Enum.Parse<Parity>(modbusConnectionInfo.Parity);
            serialPort.StopBits = Enum.Parse<StopBits>(modbusConnectionInfo.Stopbits);
            serialPort.Open();
            return serialPort;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning($"Modbus Workers is all aborted.");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Worker running at: {DateTimeOffset.Now}");
            await RunModbusReadingAnalog(hostApplicationLifetime.ApplicationStopping);
            //Task[] task = hubbubInformation.DeviceId.Select(x => RunModbusReadingAnalog(x, hostApplicationLifetime.ApplicationStopping)).ToArray();
            ////Task[] task = new Task[] { RunModbusReadingAnalog(1, hostApplicationLifetime.ApplicationStopping) };
            //Task.WaitAll(task, hostApplicationLifetime.ApplicationStopping);
            
        }

        private async Task ControlInQueue(IModbusMaster master, int slaveid)
        {
            while(globalControlQueue.TryDequeue(out ModbusControlModel result))
            {

                IEnumerable<VwDigitalOutputPoint> commmands = null;
                if(result.commandcode == ModbusCommandCodes.ACTIVE_POWER)
                {
                    int commandCode = 0;
                    if(result.commandvalue.HasValue == false || result.commandvalue == 0)
                    {
                        commandCode = (int)ModbusCommandCodes.STANDBY;
                    }
                    else if(result.commandvalue > 0)
                    {
                        commandCode = (int)ModbusCommandCodes.DISCHARGE;
                    }
                    else if(result.commandvalue < 0)
                    {
                        commandCode = (int)ModbusCommandCodes.CHARGE;
                    }

                    commmands = HubbubTemaplte.ModbusDigitalOutputPoints.Where(x => x.Devicetypeid == result.devicetype && x.Deviceindex == result.deviceindex && x.Commandcode == commandCode).OrderBy(x => x.Commandorder);
                }
                else
                {
                    commmands = HubbubTemaplte.ModbusDigitalOutputPoints.Where(x => x.Devicetypeid == result.devicetype && x.Deviceindex == result.deviceindex && x.Commandcode == (int)result.commandcode).OrderBy(x => x.Commandorder);

                }
                foreach(var command in commmands)
                {
                    ushort write_value = 0;
                    if (command.Outputvalue.HasValue)
                        write_value = Convert.ToUInt16(command.Outputvalue.Value);
                    else if(result.commandvalue.HasValue)
                    {
                        short sValue = Convert.ToInt16(command.Scalefactor != 0 ? result.commandvalue.Value * command.Scalefactor : result.commandvalue.Value);
                        write_value = (ushort)sValue;
                    }

                    switch (command.Functioncode)
                    {
                        case 6: // Write Single Holding Register
                            await master.WriteSingleRegisterAsync((byte)slaveid, (ushort)command.Offset, write_value);
                            break;
                        case 16:
                            await master.WriteMultipleRegistersAsync((byte)slaveid, (ushort)command.Offset, new ushort[] { write_value });
                            break;
                        default:
                            continue;
                    }
                    await Task.Delay(1000);
                }
            }
        }

        private async Task ReadAnalogModbusAsync(int functionCode, IModbusMaster master, int slaveid, IEnumerable<VwModbusInputPoint> points)
        {
            if (points.Count() == 0)
                return;

          
            ushort startAddress = (ushort)points.Min(x => x.Offset);
            ushort lastOffset = (ushort)points.Max(x => x.Offset);
            ushort numOfPoints = (ushort)(points.FirstOrDefault(x => x.Offset == lastOffset).Sizebyword + (lastOffset - startAddress));
            List<ushort> buffer_list = new List<ushort>();
            ushort lengthOfPoints = numOfPoints;
            while (lengthOfPoints > 0)
            {
                ushort readNumOfPoints = lengthOfPoints;
                if (lengthOfPoints >= MAX_READ_MODBUS_COUNT)
                {
                    readNumOfPoints = MAX_READ_MODBUS_COUNT;
                }

                ushort[] dataOfPoints = null;
                switch(functionCode)
                {
                    case 3:
                        dataOfPoints = await master.ReadInputRegistersAsync((byte)slaveid, startAddress, readNumOfPoints);
                        break;
                    case 4:
                        dataOfPoints = await master.ReadHoldingRegistersAsync((byte)slaveid, startAddress, readNumOfPoints);
                        break;
                    default:
                        return;
                }
                buffer_list.AddRange(dataOfPoints);
                lengthOfPoints = (ushort)(lengthOfPoints - readNumOfPoints);
            }

            ushort[] buffers = buffer_list.ToArray();
#if USE_REDIS
            Dictionary<string, List<HashEntry>> pairs = new Dictionary<string, List<HashEntry>>();
            foreach (var point in template.AnalogInputPoints.Where(x=>x.Functioncode == functionCode))
            {
                int offset = point.Offset - startAddress;
                ushort[] readData = new ushort[point.Sizebyword];
                Array.Copy(buffers, offset, readData, 0, point.Sizebyword);
                IComparable readValue = ParseReadData(readData, (DataTypes)point.Datatypeid);
                readValue = (dynamic)readValue * point.Scalefactor;
                HashEntry hashEntry = new HashEntry(point.Fieldname, readValue.ToString());
                string key = $"{point.Deviceid}.{deviceid}";
                if (pairs.ContainsKey(key) == false)
                    pairs.Add(key, new List<HashEntry>());
                pairs[key].Add(hashEntry);
            }

            foreach (string key in pairs.Keys)
            {
                await database.HashSetAsync(key, pairs[key].ToArray());
            }
#else


            //Dictionary<int, IComparable> values = new Dictionary<int, IComparable>();
            //Dictionary<DeviceTypes, JObject> AI_Models = new Dictionary<DeviceTypes, JObject>();
            //Dictionary<DeviceTypes, JObject> DI_Models = new Dictionary<DeviceTypes, JObject>();

            TwoPairKeyHashCube<int, DeviceTypes, JObject> AI_MODELS = new TwoPairKeyHashCube<int, DeviceTypes, JObject>();
            TwoPairKeyHashCube<int, DeviceTypes, JObject> DI_MODELS = new TwoPairKeyHashCube<int, DeviceTypes, JObject>();
            TwoPairKeyHashCube<int, DeviceTypes, JObject> ST_MODELS = new TwoPairKeyHashCube<int, DeviceTypes, JObject>();

            TwoPairKeyHashCube<int, int, ushort> DI_ST_Values = new TwoPairKeyHashCube<int, int, ushort>();
            StreamWriter sw = null;
            try
            {
                if (hubbubInformation.use_packet_snapshot)
                sw = new StreamWriter("Packet.txt", true, Encoding.UTF8);
                if(sw != null)
                    await sw.WriteLineAsync($"---------------{DateTime.Now.ToLongTimeString()}----------------");
                foreach (var point in points)
                {
                    DeviceTypes deviceTypes = (DeviceTypes)point.Deviceid;
                    int offset = point.Offset - startAddress;
                    ushort[] readData = new ushort[point.Sizebyword];
                    Array.Copy(buffers, offset, readData, 0, point.Sizebyword);
                    IComparable readValue = ParseReadData(readData, (DataTypes)point.Datatypeid);
                    //point.SetValue(readValue);
                    //point.Value = readValue;

                    if(sw != null)
                    {
                        await sw.WriteLineAsync($"{point.Hubbubid},{point.Name},{functionCode},{point.Offset},{point.Deviceid},{readValue}");
                    }

                    JObject model = null;
                    PointTypes pointTypes = (PointTypes)point.Pointtypeid;
                    switch (pointTypes)
                    {
                        case PointTypes.AI:
                            if(AI_MODELS.ContainsKey(point.Deviceindex, deviceTypes) == false)
                            {
                                model = PushModelFactory.CreateAnalogModel(deviceTypes, point.Deviceindex, HubbubTemaplte.Hubbub.Siteid, 
                                    hubbubInformation.rcc, HubbubTemaplte.StandardAnalogPoints);
                                AI_MODELS[point.Deviceindex, deviceTypes] = model;
                            }
                            else
                            {
                                model = AI_MODELS[point.Deviceindex, deviceTypes];
                            }
                            readValue = (dynamic)readValue * point.Scalefactor;
                            AddOrUpdateModelValue(model, point.Name, readValue);
                            break;
                        case PointTypes.DI:
                            if (DI_MODELS.ContainsKey(point.Deviceindex, deviceTypes) == false)
                            {
                                model = PushModelFactory.CreateDigitalModel(deviceTypes, point.Deviceindex, HubbubTemaplte.Hubbub.Siteid,
                                    hubbubInformation.rcc);
                                DI_MODELS[point.Deviceindex, deviceTypes] = model;
                            }
                            else
                            {
                                model = DI_MODELS[point.Deviceindex, deviceTypes];
                            }
                            DI_ST_Values.AddValueOrUpdate(point.Deviceindex, point.GetGroupCode(), (ushort)readValue);
                            EnqueueEventModel(deviceTypes, point.Deviceindex, (ushort)readValue, point.Offset);
                            AddOrUpdateModelValue(model, point.GetGroupCode().ToString(), readValue);
                            break;

                        case PointTypes.ST:
                            //if (ST_MODELS.ContainsKey(point.Deviceindex, deviceTypes) == false)
                            //{
                            //    model = PushModelFactory.CreateSTModel(deviceTypes, point.Deviceindex, HubbubTemaplte.Hubbub.Siteid,
                            //        hubbubInformation.rcc);
                            //    ST_MODELS[point.Deviceindex, deviceTypes] = model;
                            //}
                            //else
                            //{
                            //    model = ST_MODELS[point.Deviceindex, deviceTypes];
                            //}
                            //AddOrUpdateModelValue(model, point.GetGroupCode().ToString(), readValue);
                            DI_ST_Values.AddValueOrUpdate(point.Deviceindex, point.GetGroupCode(), (ushort)readValue);
                            //point.SetValue(readValue);
                            break;
                    }
                }
                foreach(int deviceindex in AI_MODELS.GetKeys())
                foreach (DeviceTypes t in AI_MODELS.GetKeys(deviceindex))
                {
                    PushModel pushModel = PushModel.CreateAnalogInputPushModel(AI_MODELS[deviceindex, t], HubbubTemaplte.Hubbub.Siteid, (int)t, deviceindex);
                    globalQueue.Enqueue(pushModel);
                    //if (deviceindex == 3)
                    //    Console.WriteLine(AI_Models[t]);
                }

                foreach (int deviceindex in DI_MODELS.GetKeys())
                foreach (DeviceTypes t in DI_MODELS.GetKeys(deviceindex))
                {
                    PushModel pushModel = PushModel.CreateDigitalInputPushModel(DI_MODELS[deviceindex, t], HubbubTemaplte.Hubbub.Siteid, (int)t, deviceindex);
                    globalQueue.Enqueue(pushModel);



                        
                }

                foreach (int deviceindex in DI_ST_Values.GetKeys())
                {
                    JObject model = PushModelFactory.CreatePcsStatusModel(deviceindex, HubbubTemaplte.Hubbub.Siteid, hubbubInformation.rcc,
                        HubbubTemaplte.StandardPcsStatuses,
                        HubbubTemaplte.ModbusDigitalStatusPoints.Where(x=>x.Deviceindex == deviceindex).ToDictionary(key=>key.Pcsstatusid, value=>value),
                        DI_ST_Values[deviceindex]
                        );

                    PushModel pushModel = PushModel.CreateDigitalStatusModel(model, HubbubTemaplte.Hubbub.Siteid, 0, deviceindex);
                    globalQueue.Enqueue(pushModel);
                    //foreach (int groupCode in DI_ST_Values.GetKeys(deviceindex))
                    //{
                    //    var dsPoints = HubbubTemaplte.ModbusDigitalStatusPoints.Where(x => x.Deviceindex == deviceindex).ToDictionary(key => key.Pcsstatusid, value => value);

                    //}
                }
                

                //foreach (int deviceindex in DI_ST_Values.GetKeys())
                //    foreach (int groupCode in DI_ST_Values.GetKeys(deviceindex))
                //    {
                //        JObject statModel = PushModelFactory.CreateSTModel(deviceindex, HubbubTemaplte.Hubbub.Siteid,
                //                hubbubInformation.rcc);
                //        PushModel pushModel = PushModel.CreateDigitalInputPushModel(statModel, HubbubTemaplte.Hubbub.Siteid, (int)t, deviceindex);
                //        globalQueue.Enqueue(pushModel);
                //        //if (deviceindex == 3)
                //        //    Console.WriteLine(AI_Models[t]);
                //    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
                finally
                {
                    if (sw != null)
                        sw.Close();
                }

            //foreach (DeviceTypes t in DI_Models.Keys)
            //{
            //    Console.WriteLine(DI_Models[t]);
            //}
            //JObject pcs_pushModel = CreatePushModel(deviceindex, DeviceTypes.PCS, DataCategory.AI);
            //JObject bms_pushModel = CreatePushModel(deviceindex, DeviceTypes.BMS, DataCategory.AI);
            //JObject pv_pushModel = CreatePushModel(deviceindex, DeviceTypes.PV, DataCategory.AI);



            //Console.WriteLine(deviceindex);
            //Console.WriteLine(pcs_pushModel);
            //Console.WriteLine(bms_pushModel);
#endif



            //_logger.LogInformation($"[Worker:{deviceid}] READ MODBUS FC({functionCode}) reading at: {DateTimeOffset.Now}");
        }

        private void EnqueueEventModel(DeviceTypes t, int deviceindex, ushort bitFlag, int groupcode)
        {
            //string topicName = $"hubbub/{SiteId}/{DeviceType}/{DeviceIndex}/Event";
            EventModel record = new EventModel();
            record.UnixTimestamp = DateTimeOffset.Now.AddHours(9).ToFileTime();
            record.DeviceType = (int)t;
            record.DeviceIndex = deviceindex;
            record.SiteId = HubbubTemaplte.Hubbub.Siteid;
            record.FactoryCode = HubbubTemaplte.Hubbub.Factorycode;
            record.Status = EventStatus.New;
            record.BitFlag = bitFlag;
            record.GroupCode = groupcode;
            globalEventQueue.Enqueue(record);
        }

        private void FIllPcsStatus(JObject obj, int groupCode, ushort StatValue)
        {

        }

        private void AddOrUpdateModelValue(JObject model, string pointName, IComparable Value)
        {
            if (model.ContainsKey(pointName))
                model[pointName].Replace(Value.ToString());
            else
                model.Add(pointName, Value.ToString());
        }
        
        private async Task RunModbusReadingAnalog(CancellationToken cancellationToken)
        {
            var ReadInputRegistPoints = HubbubTemaplte.ModbusInputPointList.Where(x => x.Functioncode == 3);
            var ReadHoldingRegistPoints = HubbubTemaplte.ModbusInputPointList.Where(x => x.Functioncode == 4);
            bool MUST_REPORT_ZK = true;

            IModbusMaster master = null;
            while (!cancellationToken.IsCancellationRequested)
            {
                
                try
                {
                    if (master == null)
                    {
                        master = CreateModbusMaster(modbusFactory, HubbubTemaplte.ConnectionInfo);
                    }

                    await ControlInQueue(master, HubbubTemaplte.ConnectionInfo.Slaveid);

                    if (ReadInputRegistPoints.Count() > 0)
                    {
                        await ReadAnalogModbusAsync(3, master, HubbubTemaplte.ConnectionInfo.Slaveid, ReadInputRegistPoints);
                        //await Task.Delay(1000, hostApplicationLifetime.ApplicationStopping);
                    }
                    if (ReadHoldingRegistPoints.Count() > 0)
                    {
                        await ReadAnalogModbusAsync(4, master, HubbubTemaplte.ConnectionInfo.Slaveid, ReadHoldingRegistPoints);
                        //await Task.Delay(1000, hostApplicationLifetime.ApplicationStopping);
                    }
                    //if (IsReadInputRegist)
                    //    await ReadAnalogModbusAsync(3, deviceid, master, template);
                    //if(IsReadHoldingRegist)
                    //    await ReadAnalogModbusAsync(4, deviceid, master, template);
                    if (MUST_REPORT_ZK)
                    {
                        //await ZkFactory.ReportHubbubStatusAsync(ZkServerHost, hubbubInformation.siteid, deviceindex, enStatus.RUN, hostApplicationLifetime.ApplicationStopping);
                        MUST_REPORT_ZK = false;
                    }
                }
                //catch (System.Net.Sockets.SocketException socex)
                //{
                //    _logger.LogError(socex, $"[Worker:{deviceid}] {socex.Message}");
                //    //if (socex.SocketErrorCode == SocketError.ConnectionRefused && master != null)
                //    //{
                //    //    master.Dispose();
                //    //    modbusMasters.TryRemove(deviceid, out master);
                //    //}
                //    await Task.Delay(5000, hostApplicationLifetime.ApplicationStopping);
                //}
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[Worker] {ex.Message}");
                    if (ex.Source == "System.Net.Sockets" && master != null)
                    {
                        master.Dispose();
                        master = null;
                    }

                    //await ZkFactory.ReportHubbubStatusAsync(ZkServerHost, hubbubInformation.siteid, deviceindex, enStatus.ERROR, hostApplicationLifetime.ApplicationStopping);
                    //MUST_REPORT_ZK = true;
                    //_logger.LogError(ioex, ioex.Message);
                    await Task.Delay(5000, hostApplicationLifetime.ApplicationStopping);
                    //await Task.Delay(100, hostApplicationLifetime.ApplicationStopping);
                }
                await Task.Delay(1000, hostApplicationLifetime.ApplicationStopping);
            }

            if(master != null)
            {
                master.Dispose();
                master = null;
            }

            _logger.LogWarning($"Modbus Worker is aborted.");

        }

        private IComparable ParseReadData(ushort[] readData, DataTypes type)
        {
            switch (type)
            {
                case DataTypes.INT16:
                    Int16 i16Value = (short)readData[0];
                    return i16Value;
                case DataTypes.INT32:
                    Int32 i32Value = (Int32)ModbusUtility.GetUInt32(readData[0], readData[1]);
                    return i32Value;
                case DataTypes.UINT32:
                    UInt32 ui32Value = ModbusUtility.GetUInt32(readData[0], readData[1]);
                    return ui32Value;
                case DataTypes.FLOAT:
                    float fValue = ModbusUtility.GetSingle(readData[1], readData[0]);
                    return fValue;
                case DataTypes.DOUBLE:
                    double dValue = ModbusUtility.GetDouble(readData[3], readData[2], readData[1], readData[0]);
                    return dValue;
                default: // U16
                    ushort u16Value = readData[0];
                    return u16Value;
            }
        }
    }
}
