using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using NLog;
using PeiuPlatform.App;
using PeiuPlatform.Models;

namespace PeiuPlatform
{
    public class DataPublisherWorker : BackgroundService
    {
        private readonly ILogger<DataPublisherWorker> _logger;
        private readonly IPacketSubscriber subscriber;
        private IMqttClient mqtt_client;

        public DataPublisherWorker(ILogger<DataPublisherWorker> logger, IPacketSubscriber subscriber)
        {
            _logger = logger;
            this.subscriber = subscriber;
            
        }

        protected async Task<bool> TryConnecting()
        {
            MqttClientOptions options = CreateMqttOption();
            if (mqtt_client == null)
            {
                mqtt_client = new MqttFactory().CreateMqttClient();
            }


            var result = await mqtt_client.ConnectAsync(options);

            return result.ResultCode == MqttClientConnectResultCode.Success;
        }

        private MqttClientOptions CreateMqttOption()
        {
            string BindAddress = Environment.GetEnvironmentVariable("ENV_PEIU_MQTT_HOST");
            int Port = int.Parse(Environment.GetEnvironmentVariable("ENV_PEIU_MQTT_PORT"));
            var ClientOptions = new MqttClientOptions
            {
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = BindAddress,
                    Port = Port,
                }


            };
            return ClientOptions;
        }

        private void ReadMembers(object obj, string Alias, IDictionary<object, object> properties)
        {
            try
            {
                foreach (FieldInfo property in obj.GetType().GetFields())
                {
                    if (property.FieldType.IsValueType && property.FieldType.IsEnum == false && property.FieldType.IsPrimitive == false)
                    {
                        ReadMembers(property.GetValue(obj), string.IsNullOrEmpty(Alias)
                            ? property.Name
                            : Alias + "." + property.Name, properties);
                    }
                    else if (properties.ContainsKey(property.Name) == false)
                    {
                        properties.Add(string.IsNullOrEmpty(Alias)
                            ? property.Name
                            : Alias + "." + property.Name, property.GetValue(obj));
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            if (await TryConnecting() == false)
                return;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    
                    if (subscriber.PublishQueues.TryDequeue(out DataMessage datamessage))
                    {
                        MqttApplicationMessage message = datamessage.Message;
                        DateTime timestamp = datamessage.Timestamp;

                        string logger = datamessage.LoggerNameByCategory;
                        if(datamessage.Category != 6) // 기온 습도는 아직 브로커에 전달하지 않음
                        {
                            await mqtt_client.PublishAsync(message, stoppingToken);

                            if (datamessage.StatusMessage != null)
                                await mqtt_client.PublishAsync(datamessage.StatusMessage, stoppingToken);
                        }

                        var data_logger = NLog.LogManager.GetLogger(logger);
                        LogEventInfo logEvent = new LogEventInfo(NLog.LogLevel.Info, logger, "");
                        logEvent.TimeStamp = timestamp.ToUniversalTime();
                        

                        ReadMembers(datamessage.Data, null, logEvent.Properties);
                        logEvent.Message = "DataLog";
                        data_logger.Log(logEvent);
                        _logger.LogInformation("Log Write..");
                    }

                    if (subscriber.PublishEventQueues.TryDequeue(out EventModel eventModel))
                    {
                        string evtMessage = JsonConvert.SerializeObject(eventModel);
                        var msg = ModelConverter.CreateMessage(evtMessage, eventModel.GetTopicName());
                        await mqtt_client.PublishAsync(msg, stoppingToken);
                    }
                }
                catch(MQTTnet.Exceptions.MqttCommunicationException mqttex)
                {
                    mqtt_client.Dispose();
                    mqtt_client = null;
                    await TryConnecting();
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }

                await Task.Delay(100, stoppingToken);
            }
        }



        private void DataLogging<T>(T source)
        {
            Logger log = LogManager.GetCurrentClassLogger();
            
            LogEventInfo logEvent = new LogEventInfo(NLog.LogLevel.Info, "data_logger", "");
            PropertyInfo[] infos = typeof(T).GetProperties();
            foreach(var p in infos)
            {
                logEvent.Properties[p.Name] = p.GetValue(source);
            }
            log.Log(logEvent);
        }
    }
}
