using PEIU.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using PES.Toolkit.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PES.Service.DataService
{
    public class MQTTDaegunSubscribe : IDisposable
    {
        readonly MqttSubscribeConfig mqttOptions;
        readonly ILogger<MQTTDaegunSubscribe> _logger;
        IMqttClient client;
       

#if RASPIAN
        //    <logger name = "record.pcs" levels="Info" writeTo="pcs" />
        //<logger name = "record.bsc" levels="Info" writeTo="bsc" />
        //<logger name = "record.pvmeter" levels="Info" writeTo="pvmeter" />
        //<logger name = "record.bscmeter" levels="Info" writeTo="bscmeter" />
        const string PCS_LOG = "record.pcs";
        const string BSC_LOG = "record.bsc";
        const string PV_METER_LOG = "record.pvmeter";
        const string BSC_METER_LOG = "record.bscmeter";
        
        readonly NLog.ILogger pcsLogger =
           NLog.LogManager.Configuration.LogFactory.GetLogger(PCS_LOG);
        readonly NLog.ILogger bscLogger = NLog.LogManager.Configuration.LogFactory.GetLogger(BSC_LOG);
        readonly NLog.ILogger pvMeterLogger = NLog.LogManager.Configuration.LogFactory.GetLogger(PV_METER_LOG);
        readonly NLog.ILogger bscMeterLogger = NLog.LogManager.Configuration.LogFactory.GetLogger(BSC_METER_LOG);

        public MQTTDaegunSubscribe(ILoggerFactory logger, MqttSubscribeConfig mqtt_config)
        {
            _logger = logger.CreateLogger< MQTTDaegunSubscribe>();
            mqttOptions = mqtt_config;
            StartSubscribe();
        }
#else
        readonly IBackgroundMongoTaskQueue queue;
        public MQTTDaegunSubscribe(ILoggerFactory loggerFactory, IBackgroundMongoTaskQueue taskQueue, MqttSubscribeConfig mqtt_config)
        {
            mqttOptions = mqtt_config;
            queue = taskQueue;
            _logger = loggerFactory.CreateLogger<MQTTDaegunSubscribe>();
            StartSubscribe();
        }
#endif

        private async void StartSubscribe()
        {
            var ClientOptions = new MqttClientOptions
            {
                ClientId = mqttOptions.ClientId,
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = mqttOptions.BindAddress,
                    Port = mqttOptions.Port
                },
                
            };

            client = new MqttFactory().CreateMqttClient();
            client.ApplicationMessageReceived += ManagedClient_ApplicationMessageReceived;
            client.Connected += ManagedClient_Connected;
            client.Disconnected += Client_Disconnected;

            try
            {
                var result =  await client.ConnectAsync(ClientOptions);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "### CONNECTING FAILED ###" + Environment.NewLine + exception);
            }
        }

        private async void Client_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            _logger.LogWarning($"### DISCONNECTED FROM SERVER. TRY CONNECT AFTER {mqttOptions.RecordInterval} ###");
            await Task.Delay(mqttOptions.RecordInterval);

            try
            {
                var ClientOptions = new MqttClientOptions
                {
                    ClientId = mqttOptions.ClientId,
                    ChannelOptions = new MqttClientTcpOptions
                    {
                        Server = mqttOptions.BindAddress,
                        Port = mqttOptions.Port
                    }
                };
                await client.ConnectAsync(ClientOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "### RECONNECTING FAILED ###");
            }
        }

        private async void ManagedClient_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            _logger.LogInformation($"### CONNECTED WITH SERVER (TOPIC FILTER:{mqttOptions.Topic}) ###");
            await client.SubscribeAsync(new TopicFilterBuilder().WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)mqttOptions.QoSLevel).WithTopic(mqttOptions.Topic).Build());
            
        }

        //Dictionary<int, DateTime> lastRecordTime = new Dictionary<int, DateTime>();

        //Dictionary<int, DateTime> lastRecordTime = new Dictionary<int, DateTime>();
        private void ManagedClient_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                
                //lastTime = DateTime.Now.Add(mqttOptions.RecordInterval);

                //byte[] pay_load = e.ApplicationMessage.Payload;
                //byte[] new_fileArray = new byte[pay_load.Length + 8];
                //Array.Copy(pay_load, new_fileArray, pay_load.Length);
                //File.WriteAllBytes($"{DateTime.Now.TimeOfDay}_dump.bin", e.ApplicationMessage.Payload);
                DaegunPacket packet = PacketParser.ByteToStruct<DaegunPacket>(e.ApplicationMessage.Payload);
                //if (lastRecordTime.ContainsKey(packet.sSiteId) == false)
                //    lastRecordTime.Add(packet.sSiteId, DateTime.MinValue);
                //if (DateTime.Now < lastRecordTime[packet.sSiteId])
                //    return;
                _logger.LogInformation($"RECEIVED DAEGUN : siteid({packet.sSiteId}) from({e.ClientId}) t({e.ApplicationMessage.Topic}) QoS({e.ApplicationMessage.QualityOfServiceLevel}) size({e.ApplicationMessage.Payload.Length})");
                //nLogger.Info($"RECEIVED DAEGUN : siteid({packet.sSiteId}) from({e.ClientId}) t({e.ApplicationMessage.Topic}) QoS({e.ApplicationMessage.QualityOfServiceLevel}) size({e.ApplicationMessage.Payload.Length})");
                //lastRecordTime[packet.sSiteId] = DateTime.Now.Add(mqttOptions.RecordInterval);
                //packet.timestamp = DateTime.Now;
                DateTime dt = DateTime.Now.Date
                    .AddHours(DateTime.Now.Hour)
                    .AddMinutes(DateTime.Now.Minute)
                    .AddSeconds(DateTime.Now.Second);

#if RASPIAN == false
            queue.QueueBackgroundWorkItem(new DaegunPacketClass(packet, dt));
#else
                NLog.LogEventInfo pcsEventInfo = LogEventMaker.CreateLogEvent(PCS_LOG, packet.Pcs);
                NLog.LogEventInfo bscEventInfo = LogEventMaker.CreateLogEvent(BSC_LOG, packet.Bsc);
                NLog.LogEventInfo pvEventInfo = LogEventMaker.CreateLogEvent(PV_METER_LOG, packet.Pv);
                NLog.LogEventInfo essEventInfo = LogEventMaker.CreateLogEvent(BSC_METER_LOG, packet.Ess);
                //NLog.Logger logger =  NLog.LogManager.Configuration.LogFactory.GetLogger("record.pcs");
                //NLog.LogEventInfo logEvent = LogEventMaker.CreateLogEvent("record.pcs", pcsPacket);
                pcsEventInfo.Properties["SiteId"] =
                    bscEventInfo.Properties["SiteId"] =
                    pvEventInfo.Properties["SiteId"] =
                    essEventInfo.Properties["SiteId"] = packet.sSiteId;
                pcsLogger.Log(pcsEventInfo);
                bscLogger.Log(bscEventInfo);
                pvMeterLogger.Log(pvEventInfo);
                bscMeterLogger.Log(essEventInfo);
                //logger.Log(logEvent);

#endif
                //logger.LogInformation("Store Daegun Packet");
                //if(lastRecordTime.ContainsKey(packet.sSiteId) == false)
                //{
                //    lastRecordTime.Add(packet.sSiteId, DateTime.MinValue);
                //}

                //if (DateTime.Now > lastRecordTime[packet.sSiteId])
                //{

                //    lastRecordTime[packet.sSiteId] = DateTime.Now.Add(mqttOptions.RecordInterval);
                //}

            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                _logger.LogError(ex, ex.Message);
            }
        }

        public void Dispose()
        {
            if (client != null)
                client.Dispose();
        }
    }
}
