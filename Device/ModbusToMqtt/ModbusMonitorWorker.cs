using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NModbus;
using Power21.Device.Dao;
using Power21.Device.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Power21.Device
{
    public class ModbusMonitorWorker : BackgroundService
    {
        private readonly ILogger<ModbusMonitorWorker> logger;
        private readonly IModbusInterface valueQueue;
        private readonly ModbusConfig modbusConfig;
        readonly IMqttQueue mqttQueue;
        readonly Map map;
        public ModbusMonitorWorker(ILogger<ModbusMonitorWorker> logger, IModbusInterface valueQueue, IMqttQueue controlQueue, ModbusConfig config, Map map)
        {
            this.logger = logger;
            this.valueQueue = valueQueue;
            this.modbusConfig = config;
            this.map = map;
            this.mqttQueue = controlQueue;
        }

        private async Task<TcpClient> TryConnecting(CancellationToken stoppingToken)
        {

            while(stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    TcpClient client = new TcpClient(this.modbusConfig.Address, this.modbusConfig.Port);
                    logger.LogWarning("### CONNECTED WITH MODBUS SERVER ###");
                    return client;
                }
                catch(Exception ex)
                {
                    logger.LogError("### RECONNECTING TO MODBUS SLAVE FAILED ###");
                    logger.LogInformation("TRY CONNECTING AFTER 5 SECONDS");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
            return null;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (this.modbusConfig.ProtocolType != ProtocolType.Tcp)
                throw new NotSupportedException("현재 모드버스는 TCP만 지원합니다");
            await Task.Delay(100, stoppingToken);
            TcpClient client = null;
            //if(client == null)
            //{
            //    logger.LogWarning("## MODBUS Monitor worker is break");
            //    return;
            //}
            var factory = new ModbusFactory();
            IModbusMaster master = null;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (client == null)
                    {
                        client = await TryConnecting(stoppingToken);
                        if (client == null)
                        {
                            logger.LogWarning("## MODBUS Monitor worker is break");
                            return;
                        }
                        master = factory.CreateMaster(client);
                    }
                    foreach (MapRow row in map.Rows)
                    {

                        while (true)
                        {
                            (MapControl mc, IComparable value) result = await mqttQueue.DequeueAsync(modbusConfig.PollInterval, stoppingToken);

                            if (result.mc != null)
                            {
                                FunctionCode fc = (FunctionCode)result.mc.FunctionCode;
                                MapPoint mapPoint = result.mc.Point;
                                ushort[] buffers = mapPoint.ToBytesValue(result.value);
                                if (buffers != null)
                                {
                                    switch (fc)
                                    {
                                        case FunctionCode.ReadHoldingRegisters:

                                            await master.WriteMultipleRegistersAsync(0, mapPoint.Offset, buffers);
                                            logger.LogWarning($"## Writing {result.mc.Point.Name} = {result.value} point to modbus ##");
                                            break;
                                    }
                                }
                            }
                            else
                                break;
                        }
                        ushort startAddress = row.StartAddress();
                        ushort numInputs = row.NumOfPoints();
                        if (numInputs > 125)
                        {
                            logger.LogWarning("한번에 가져올 수 없는 포인트 수는 125개를 넘을 수 없습니다");
                            continue;
                        }

                        ushort[] buffer = null;

                        FunctionCode FC = (FunctionCode)row.FunctionCode;
                        switch (FC)
                        {
                            case FunctionCode.ReadHoldingRegisters:
                                buffer = await master.ReadHoldingRegistersAsync(0, startAddress, numInputs);
                                break;
                        }

                        UpdatePoints(buffer, row);

                        

                        //await Task.Delay(modbusConfig.PollInterval, stoppingToken);

                    }
                }
                catch(IOException ioex)
                {
                    if(ioex.Source == "System.Net.Sockets")
                    {
                        client.Dispose();
                        client = null;
                    }
                    logger.LogError(ioex, ioex.Message);
                    await Task.Delay(modbusConfig.Timeout, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                    await Task.Delay(modbusConfig.Timeout, stoppingToken);
                }
            }

            client.Close();
        }

        private void UpdatePoints(ushort[] buffers, MapRow row)
        {
            ushort startOffset = row.StartAddress();
            foreach(MapPoint point in row.Points)
            {
                ushort offset = (ushort)(point.Offset - startOffset);
                IComparable value = 0;
                switch(point.PointType)
                {
                    case PointType.INT16:
                        value = (short)buffers[offset];
                        break;
                    case PointType.UINT16:
                        value = buffers[offset];
                        break;
                    case PointType.INT32:
                        ushort[] dword_buffer = new ushort[2];
                        Array.Copy(buffers, offset, dword_buffer, 0, 2);
                        value = ConvertDecimalToInt32(dword_buffer);
                        break;
                    case PointType.UINT32:
                        dword_buffer = new ushort[2];
                        Array.Copy(buffers, offset, dword_buffer, 0, 2);
                        value = ConvertDecimalToUint32(dword_buffer);
                        break;
                    case PointType.FLOAT:
                        dword_buffer = new ushort[2];
                        Array.Copy(buffers, offset, dword_buffer, 0, 2);
                        value = ConvertDecimalToIEEE754float(dword_buffer);
                        break;
                }

                if (point.Scale != 0)
                    value = (dynamic)value * point.Scale;
                //logger.LogInformation($"{point.Name} : {value}");
                valueQueue.SetValue(point.Name, value);
            }
        }

        private byte[] ConvertBytes(ushort[] svalue)
        {
            byte[] fbits = new byte[svalue.Length * 2];
            int idx = fbits.Length - 2;
            foreach (ushort ushort_str in svalue)
            {
                ushort usvalue = ushort.Parse(ushort_str.ToString());
                byte[] bits = BitConverter.GetBytes(usvalue);
                Array.Copy(bits, 0, fbits, idx, 2);
                idx -= 2;
            }
            return fbits;
        }

        UInt32 ConvertDecimalToUint32(ushort[] svalue)
        {
            byte[] fbits = ConvertBytes(svalue);
            UInt32 value = BitConverter.ToUInt32(fbits, 0);
            return value;
        }

        Int32 ConvertDecimalToInt32(ushort[] svalue)
        {
            byte[] fbits = ConvertBytes(svalue);
            Int32 value = BitConverter.ToInt32(fbits, 0);
            return value;
        }

        float ConvertDecimalToIEEE754float(ushort[] svalue)
        {
            byte[] fbits = ConvertBytes(svalue);
            float value = BitConverter.ToSingle(fbits, 0);
            return value;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Reading Worker stoping at: {time}", DateTimeOffset.Now);
            return base.StopAsync(cancellationToken);
        }
    }
}
