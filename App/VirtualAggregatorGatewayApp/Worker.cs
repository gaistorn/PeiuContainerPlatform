using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using Newtonsoft.Json.Linq;
using NModbus;
using PeiuPlatform.Models;

namespace  PeiuPlatform

{
    public class Worker : BackgroundService
    {
        private (string host, int port)[] ModbusInfos = null;
        private readonly ILogger<Worker> _logger;
        private readonly IGlobalStorage globalStorage;

        private ConcurrentDictionary<int, (TcpClient client, IModbusMaster master)> clients = 
            new ConcurrentDictionary<int, (TcpClient client, IModbusMaster master)>();


        public Worker(ILogger<Worker> logger, IGlobalStorage globalStorage)
        {
            _logger = logger;
            this.globalStorage = globalStorage;
            InitModbusInfos();
        }

        private void InitModbusInfos()
        {
            string env_modbus = Environment.GetEnvironmentVariable("ENV_MODBUSES");
            string[] devices = env_modbus.Split(',');
            ModbusInfos = new (string, int)[devices.Length];
            for(int i=0;i<devices.Length;i++)
            {
                string[] infos = devices[i].Split(':');

                string host = infos[0];
                int port = int.Parse(infos[1]);
                ModbusInfos[i] = (host, port);
            }
        }

        private void GetModbusClient(ModbusFactory factory, int deviceindex, ref TcpClient client, ref IModbusMaster master)
        {
            if(clients.ContainsKey(deviceindex))
            {
                client = clients[deviceindex].client;
                master = clients[deviceindex].master;
            }
            else
            {
                string host = ModbusInfos[deviceindex].host;
                int port = ModbusInfos[deviceindex].port;

                client = new TcpClient(host, port);
                master = factory.CreateMaster(client);
                clients[deviceindex] = (client, master);

                string timestamp_string = DateTime.Now.ToString("yyyyMMddHHmmss");
                //clients.Add(deviceindex, (client, master));
            }
        }

        private DataMessage CreatePcsStatusMessage(ushort statusValue, DateTime timestamp, int pcsNo, int siteid, int rcc)
        {
            DataMessage msg = new DataMessage();
            msg.Data = statusValue;
            msg.DeviceType = 1;
            msg.Timestamp = timestamp;

            JObject bat_jobj = CreatePcsStatus(pcsNo, siteid, rcc, msg.TimestampString, statusValue);

            string pcs_status_topic_name = $"hubbub/{siteid}/0/{pcsNo}/stat";

            byte[] payload_buffer = System.Text.Encoding.UTF8.GetBytes(bat_jobj.ToString());
            msg.Message = new MqttApplicationMessageBuilder()
                .WithAtLeastOnceQoS()
                .WithPayload(payload_buffer)
                .WithTopic(pcs_status_topic_name)
                .Build();
            return msg;
        }

        private DataMessage CreateBatMessage(ST_BSC bsc, DateTime timestamp, int batNo, int siteid, int rcc)
        {
            DataMessage msg = new DataMessage();
            msg.Data = bsc;
            msg.DeviceType = 1;
            msg.Timestamp = timestamp;

            JObject bat_jobj = CreateBscModel(bsc, msg.TimestampString, batNo, siteid, rcc);

            string topicName = $"hubbub/{siteid}/{msg.DeviceName}{batNo}/AI";

            byte[] payload_buffer = System.Text.Encoding.UTF8.GetBytes(bat_jobj.ToString());
            msg.Message = new MqttApplicationMessageBuilder()
                .WithAtLeastOnceQoS()
                .WithPayload(payload_buffer)
                .WithTopic(topicName)
                .Build();

            //if (status != null)
            //{
            //    payload_buffer = System.Text.Encoding.UTF8.GetBytes(status.ToString());
            //    dataMessage.StatusMessage = new MqttApplicationMessageBuilder()
            //        .WithAtLeastOnceQoS()
            //        .WithPayload(payload_buffer)
            //        .WithTopic(status_topic)
            //        .Build();
            //}
            return msg;
        }

        private DataMessage CreatePcsMessage(ST_PCS pcs, DateTime timestamp, int pcsNo, int siteid, int rcc)
        {
            DataMessage msg = new DataMessage();
            msg.Data = pcs;            
            msg.DeviceType = 0;
            msg.Timestamp = timestamp;

            JObject pcs_jobj = CreatePcsModel(pcs, msg.TimestampString, pcsNo, siteid, rcc);

            string topicName = $"hubbub/{siteid}/{msg.DeviceName}{pcsNo}/AI";

            byte[] payload_buffer = System.Text.Encoding.UTF8.GetBytes(pcs_jobj.ToString());
            msg.Message = new MqttApplicationMessageBuilder()
                .WithAtLeastOnceQoS()
                .WithPayload(payload_buffer)
                .WithTopic(topicName)
                .Build();

            //if (status != null)
            //{
            //    payload_buffer = System.Text.Encoding.UTF8.GetBytes(status.ToString());
            //    dataMessage.StatusMessage = new MqttApplicationMessageBuilder()
            //        .WithAtLeastOnceQoS()
            //        .WithPayload(payload_buffer)
            //        .WithTopic(status_topic)
            //        .Build();
            //}
            return msg;
        }

        private JObject CreateBscModel(ST_BSC batPacket, string timestamp, int batNo, int siteid, int rcc)
        {
            JObject obj = CreateTemporary(2, "PCS_BATTERY", $"BMS{batNo}", siteid, rcc, timestamp);
            obj.Add("bms_soc", batPacket.SOC / 10);
            obj.Add("bms_soh", batPacket.SOH / 10);
            obj.Add("dcCellPwr", 0);
            obj.Add("dcCellVlt", batPacket.DCVoltage / 10);
            obj.Add("dcCellCrt", batPacket.DCCurrent / 100);
            obj.Add("dcCellTmpMx", batPacket.MaxModuleTemp / 10);
            obj.Add("dcCellTmpMn", batPacket.MinModuleTemp / 10);
            obj.Add("dcCellVltMx", batPacket.MaxCellVoltage / 10);
            obj.Add("dcCellVltMn", batPacket.MinCellVoltage / 10);
            return obj;
        }

        private JObject CreatePcsModel(ST_PCS pcsPacket, string timestamp, int pcsNo, int siteid, int rcc)
        {
            JObject obj = CreateTemporary(1, "PCS_SYSTEM", $"PCS{pcsNo}", siteid, rcc, timestamp);
            obj.Add("freq", pcsPacket.Frequency / 100) ;
            obj.Add("acGridVlt", 0);
            obj.Add("acGridCrtLow", 0);
            obj.Add("acGridCrtHigh", 0);
            obj.Add("acGridPwr", 0);
            obj.Add("actPwrKw", pcsPacket.ActivePower / 10);
            obj.Add("rctPwrKw", pcsPacket.ReactivePower / 10);
            obj.Add("pwrFact", pcsPacket.PF / 10);
            obj.Add("acGridVltR", pcsPacket.ACPhaseVoltage.R / 10);
            obj.Add("acGridVltS", pcsPacket.ACPhaseVoltage.S / 10);
            obj.Add("acGridVltT", pcsPacket.ACPhaseVoltage.T / 10);
            obj.Add("acGridCrtR", pcsPacket.ACPhaseCurrent.R / 100);
            obj.Add("acGridCrtS", pcsPacket.ACPhaseCurrent.S / 100);
            obj.Add("acGridCrtT", pcsPacket.ACPhaseCurrent.T / 100);
            obj.Add("actCmdLimitLowChg", 0);
            obj.Add("actCmdLimitLowDhg", 0);
            obj.Add("actCmdLimitHighChg", 0);
            obj.Add("actCmdLimitHighDhg", 0);
            return obj;
        }

        private JObject CreatePcsStatus(int deviceindex, int siteid, int rcc, string timestamp, ushort StatusValue)
        {
            JObject PCSStatusSample = new JObject();
            PCSStatusSample.Add("ditype", 1);
            PCSStatusSample.Add("devicetype", 0);
            PCSStatusSample.Add("deviceindex", deviceindex);
            PCSStatusSample.Add("RUN", BitWise(StatusValue, 1));
            PCSStatusSample.Add("STOP", BitWise(StatusValue, 2));
            PCSStatusSample.Add("STAND_BY", BitWise(StatusValue, 4));
            PCSStatusSample.Add("CHARGE", BitWise(StatusValue, 8));
            PCSStatusSample.Add("DISCHARGE", BitWise(StatusValue, 16));
            PCSStatusSample.Add("EMERGENCY", BitWise(StatusValue, 32));
            PCSStatusSample.Add("AC_CB", 0);
            PCSStatusSample.Add("AC_MC", 0);
            PCSStatusSample.Add("DC_CB", BitWise(StatusValue, 128));
            PCSStatusSample.Add("FAULT", 0);
            PCSStatusSample.Add("WARNING", 0);
            PCSStatusSample.Add("COMM", 0);
            PCSStatusSample.Add("LOCALREMOTE", 0);
            PCSStatusSample.Add("MANUALAUTO", 0);
            return PCSStatusSample;
        }

        private int BitWise(IComparable value, uint BitFlag, bool IsReverse = false)
        {
            bool result = (((dynamic)value & BitFlag) == BitFlag);
            if (IsReverse)
                return result == false ? 1 : 0;
            else
                return result ? 1 : 0;
        }

        private JObject CreateTemporary(int groupId, string groupName, string deviceId, int siteId, int rcc, string timestamp)
        {
            JObject newObj = new JObject();
            newObj.Add("groupid", groupId);
            newObj.Add("groupname", groupName);
            newObj.Add("deviceId", deviceId);
            newObj.Add("normalizedeviceid", deviceId);
            newObj.Add("siteId", siteId);
            newObj.Add("rcc", rcc);
            newObj.Add("inJeju", 0);
            newObj.Add("timestamp", timestamp);
            return newObj;

        }

        private async Task ReadPcsDataFromModbusAsync(IModbusMaster master, DateTime timestamp, int pcsNo, int siteid, int rcc)
        {
            ushort[] source = await master.ReadHoldingRegistersAsync(1, 1032, 31);
            byte[] target = new byte[source.Length * 2];
            Buffer.BlockCopy(source, 0, target, 0, source.Length);
            ST_PCS pcs = ByteToStruct<ST_PCS>(target);
            DataMessage pcs_msg = CreatePcsMessage(pcs, timestamp, pcsNo, siteid, rcc);
            globalStorage.DataMessageQueues.Enqueue(pcs_msg);
        }

        private async Task ReadBatDataFromModbusAsync(IModbusMaster master, DateTime timestamp, int batNo, int siteid, int rcc)
        {
            ushort[] source = await master.ReadHoldingRegistersAsync(1, 2026, 14);
            byte[] target = new byte[source.Length * 2];
            Buffer.BlockCopy(source, 0, target, 0, source.Length);
            ST_BSC bsc = ByteToStruct<ST_BSC>(target);
            DataMessage bsc_msg = CreateBatMessage(bsc, timestamp, batNo, siteid, rcc);
            globalStorage.DataMessageQueues.Enqueue(bsc_msg);
        }

        private async Task ReadPcsStatusFromModbusAsync(IModbusMaster master, DateTime timestamp, int pcsNo, int siteid, int rcc)
        {
            ushort[] source = await master.ReadHoldingRegistersAsync(1, 1012, 1);
            DataMessage msg = CreatePcsStatusMessage(source[0], timestamp, pcsNo, siteid, rcc);
            globalStorage.DataMessageQueues.Enqueue(msg);
        }

        private async Task WritePcsControlToModbusAsync(IModbusMaster master, ModbusControlModel model)
        {
            ushort statusContolValue = ushort.MaxValue;
            ushort ref_value = ushort.MaxValue;
            switch( model.commandcode)
            {
                case ModbusCommandCodes.CHARGE:
                    if (model.commandvalue.HasValue)
                    {
                        ref_value = Convert.ToUInt16(Math.Abs(model.commandvalue.Value * 10));
                        statusContolValue = 8;
                    }
                    break;
                case ModbusCommandCodes.DISCHARGE:
                    if (model.commandvalue.HasValue)
                    {
                        ref_value = Convert.ToUInt16(Math.Abs(model.commandvalue.Value * 10));
                        statusContolValue = 16;
                    }
                    break;
                case ModbusCommandCodes.ACTIVE_POWER:
                    if (model.commandvalue.HasValue)
                    {
                        ref_value = Convert.ToUInt16(Math.Abs(model.commandvalue.Value * 10));
                        if (model.commandvalue.Value > 0)
                        {
                            // 방전
                            statusContolValue = 16;
                        }
                        else
                        {
                            // 0도 그냥 충전으로 취급
                            statusContolValue = 8;
                        }
                    }
                    break;
                case ModbusCommandCodes.RUN:
                    statusContolValue = 1;
                    break;
                case ModbusCommandCodes.STOP:
                    statusContolValue = 2;
                    break;
                case ModbusCommandCodes.STANDBY:
                    statusContolValue = 4;
                    break;
                case ModbusCommandCodes.EMERGENCY_STOP:
                    statusContolValue = 32;
                    break;
                case ModbusCommandCodes.RESET:
                    statusContolValue = 64;
                    break;
            }

            if(statusContolValue == ushort.MaxValue)
            { 
                JObject obj = JObject.FromObject(model);
                _logger.LogError($"UNKNOWN COMMAND\n{obj.ToString()}");
                return;
            }

            ushort[] write_buffer = null;
            if (ref_value != ushort.MaxValue)
                write_buffer = new ushort[] { statusContolValue, ref_value };
            else
                write_buffer = new ushort[] { statusContolValue };

            await master.WriteMultipleRegistersAsync(1, 1200, write_buffer);

            /*
            1   0x01    RUN
            2   0x02    STOP
            4   0x03    STAND BY
            8   0x04    CHARGE
            16  0x05    DISCHARGE
            32  0x06    EMERGENCY
            64  0x07    RESET
            128 0x08    RELAY_CLOSE
            256 0x09    RELAY_OPEN
            512 0x10    BSC_EVENT_CLEAR
            1024    0x11    BSC_RESTART
            2048    0x12    ISOLATION
            4096    0x13    STOP_ISOLATION
            8192    0x14    HEART_BEAT
            16384   0x15    FR_CHARGE
            32768   0x16    FR_DISCHARGE
            */

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var modbusFactory = new ModbusFactory();
            int deviceCounts = ModbusInfos.Length;

            int siteid = int.Parse(Environment.GetEnvironmentVariable("ENV_SITE_ID"));
            int rcc = int.Parse(Environment.GetEnvironmentVariable("ENV_RCC_ID"));

            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime timestamp = DateTime.Now;
                Parallel.For(0, deviceCounts, async i =>
                {
                    TcpClient client = null;
                    IModbusMaster master = null;
                    try
                    {
                        GetModbusClient(modbusFactory, i, ref client, ref master);

                        while(globalStorage.ControlModelQueues.TryDequeue(out ModbusControlModel ctl))
                        {
                            // control 처리
                            await WritePcsControlToModbusAsync(master, ctl);
                        }

                        await ReadPcsDataFromModbusAsync(master, timestamp, i + 1, siteid, rcc);
                        await ReadBatDataFromModbusAsync(master, timestamp, i + 1, siteid, rcc);
                        await ReadPcsStatusFromModbusAsync(master, timestamp, i + 1, siteid, rcc);

                    }
                    catch (IOException ioex)
                    {
                        if (ioex.Source == "System.Net.Sockets")
                        {
                            if (client != null)
                            {
                                client.Dispose();
                                client = null;
                            }
                            if (master != null)
                            {
                                master.Dispose();
                            }
                            if (clients.ContainsKey(i))
                            {
                                (TcpClient, IModbusMaster) removeAt;
                                clients.TryRemove(i, out removeAt);
                            }
                        }
                        _logger.LogError(ioex, ioex.Message);
                        await Task.Delay(1000, stoppingToken);
                    }
                    catch (Exception ex)
                    {

                    }
                });
                for (int i = 0; i < deviceCounts; i++)
                {
                    
                }
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        static T ByteToStruct<T>(byte[] buffer) where T : struct
        {
            int iSize = Marshal.SizeOf(typeof(T));

            if (iSize > buffer.Length)
            {
                throw new Exception(string.Format("SIZE ERR(len:{0} sz:{1})", buffer.Length, iSize));
            }

            IntPtr ptr = Marshal.AllocHGlobal(iSize);
            Marshal.Copy(buffer, 0, ptr, iSize);
            T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);

            return obj;
        }

    }
}
