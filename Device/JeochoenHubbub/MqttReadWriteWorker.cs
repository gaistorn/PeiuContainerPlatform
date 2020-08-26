using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PeiuPlatform.App;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.Hubbub
{
    public class MqttReadWriteWorker : BackgroundService,
        //IMqttInterface, 
        MQTTnet.Client.Connecting.IMqttClientConnectedHandler,
        MQTTnet.Client.Disconnecting.IMqttClientDisconnectedHandler,
        MQTTnet.Client.Receiving.IMqttApplicationMessageReceivedHandler
    {
        private readonly ILogger<MqttReadWriteWorker> logger;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly IGlobalStorage<EventModel> globalStorage;
        private readonly HubbubInformation hubbubInformation;
        private readonly IMqttClient client;
        private readonly MqttClientOptions mqttClientOptions;
        private readonly Model.TemplateRoot template;

        public MqttReadWriteWorker(ILogger<MqttReadWriteWorker> logger,
            HubbubInformation hubbubInformation,
            IHostApplicationLifetime hostApplicationLifetime,
            IGlobalStorage<EventModel> globalStorage,
            MqttClientTcpOptions mqttClientTcpOptions,
            Model.TemplateRoot template)
        {
            this.logger = logger;
            this.hubbubInformation = hubbubInformation;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.globalStorage = globalStorage;
            this.template = template;

            var factory = new MqttFactory();
            this.client = factory.CreateMqttClient();
            mqttClientOptions = new MqttClientOptions
            {
                ChannelOptions = mqttClientTcpOptions,
                
            };
            //SendingQueueInterval = config.GetSection("SendingQueueInterval").Get<TimeSpan>();
            //PushTemplates = config.GetSection("SendingQueue").Get<PushTemplate[]>();
        }

        private async Task InitializeMqtt(CancellationToken cancellationToken)
        {
            client.UseApplicationMessageReceivedHandler(this);
            client.UseConnectedHandler(this);
            client.UseDisconnectedHandler(this);
            await TryConnecting(cancellationToken);
        }

        public async Task TryConnecting(CancellationToken cancellationToken)
        {
            try
            {

                await client.ConnectAsync(mqttClientOptions, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "### RECONNECTING TO MQTT BROKER FAILED ###" + Environment.NewLine + ex.Message);
                logger.LogInformation("TRY CONNECTING AFTER 5 SECONDS");
                Thread.Sleep(TimeSpan.FromSeconds(10));
                //await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        private async void TranslateCommand(PcsControlModel model)
        {
            if(model.LocalRemote.HasValue)
            {
                ModbusWriteCommand command = new ModbusWriteCommand();
                command.StartAddress = 499;
                ushort value = model.LocalRemote.Value ? (ushort)3 : (ushort)0;
                command.WriteValue = value;
                globalStorage.SetWriteValues(command);
            }

            ushort startAddress = (ushort)(600 + ((model.deviceindex - 1) * 200));
            ushort pcscommand = 0;
            if(model.StopRun.HasValue && model.StopRun == true)
            {
                pcscommand = (ushort)(pcscommand | 1);
            }
            if(model.FaultReset.HasValue && model.FaultReset == true)
            {
                pcscommand = (ushort)(pcscommand | 2);
            }
            if(model.EmergencyStop.HasValue && model.EmergencyStop == true)
            {
                pcscommand = (ushort)(pcscommand | 4);
            }
            //if(model.ManualAuto.HasValue && model.ManualAuto == true)
            //{
            //    pcscommand = (ushort)(pcscommand | 8);
            //}
            if (model.StopRun.HasValue || pcscommand != 0)
            {
                ModbusWriteCommand ccommand = new ModbusWriteCommand();
                ccommand.StartAddress = startAddress;
                ccommand.WriteValue = pcscommand;
                globalStorage.SetWriteValues(ccommand);
            }
            //ushort localRemoteValue = (ushort)await globalStorage.GetValue("PCS#COMMON.LocalRemote");
            //if (localRemoteValue == 0)
            //{
            //    logger.LogWarning("현재 PMS에 제어권이 없습니다. 제어가 실패했습니다");
            //    return;
            //}

            if (model.ManualAuto.HasValue)
            {
                ModbusWriteCommand command = new ModbusWriteCommand();
                command.StartAddress = (ushort)(startAddress + 5);
                ushort value = model.ManualAuto.Value ? (ushort)1 : (ushort)0;
                command.WriteValue = value;
                globalStorage.SetWriteValues(command);
            }
           
            if(model.ActivePower.HasValue)
            {
                ModbusWriteCommand command = new ModbusWriteCommand();
                command.StartAddress = (ushort)(startAddress + 1);
                ushort value = model.ActivePower.Value != 0 ? (ushort)(model.ActivePower.Value * 10) : (ushort)0;
                command.WriteValue = (ushort)(model.ActivePower.Value * 10);
                globalStorage.SetWriteValues(command);
            }

            if(model.SOCUpper.HasValue)
            {
                ModbusWriteCommand command = new ModbusWriteCommand();
                command.StartAddress = (ushort)(startAddress + 3);
                command.WriteValue = (ushort)(model.SOCUpper.Value);
                globalStorage.SetWriteValues(command);
            }

            if (model.SOCLower.HasValue)
            {
                ModbusWriteCommand command = new ModbusWriteCommand();
                command.StartAddress = (ushort)(startAddress + 4);
                command.WriteValue = (ushort)(model.SOCLower.Value);
                globalStorage.SetWriteValues(command);
            }
        }

        public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            logger.LogInformation("### RECEIVED APPLICATION MESSAGE ###");
            logger.LogInformation($"+ Topic = {e.ApplicationMessage.Topic}");
            logger.LogInformation($"+ Payload = {payload}");
            logger.LogInformation($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            logger.LogInformation($"+ Retain = {e.ApplicationMessage.Retain}");

            JObject model = JObject.Parse(payload);

            PcsControlModel pcsControlModel = JsonConvert.DeserializeObject<PcsControlModel>(payload);
            if(pcsControlModel.siteid != 6)
            {
                return Task.CompletedTask;
            }

            TranslateCommand(pcsControlModel);

            //template.PushModels

            //MapControl mc = map.Controls.FirstOrDefault(x => x.Topic == e.ApplicationMessage.Topic);
            //if (mc != null)
            //{
            //    IComparable value = mc.Point.ToValue(e.ApplicationMessage.Payload);
            //    queue.QueueBackgroundWorkItem((mc, value));
            //    logger.LogInformation($"+ Control = {mc.Point.Name}, Value = {value}");
            //    //e.ApplicationMessage
            //}
            return Task.CompletedTask;
        }

        public async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            logger.LogInformation("### CONNECTED WITH MQTT BROKER SERVER ###");
#if !EVENT
            await client.SubscribeAsync(CreateTopicFilter());

            logger.LogInformation("### SUBSCRIBED ###");
#endif
        }

        protected virtual TopicFilter CreateTopicFilter()
        {
            return new TopicFilterBuilder()
                 .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)2)
                 .WithTopic("hubbub/6/+/+/control").Build();
        }


        //public override Task StartAsync(CancellationToken cancellationToken)
        //{
        //    return InitializeMqtt(cancellationToken);
        //}

        public async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            if (eventArgs.Exception != null)
            {
                logger.LogError(eventArgs.Exception, eventArgs.Exception.Message);
            }
            logger.LogWarning($"### DISCONNECTED FROM MQTT BROKER SERVER ###");
            if (eventArgs.AuthenticateResult != null)
            {
                logger.LogWarning($"ResultCode: {eventArgs.AuthenticateResult.ResultCode}");
                logger.LogWarning($"Reason: {eventArgs.AuthenticateResult.ReasonString}");
            }
            //if(eventArgs.ClientWasConnected == false)
            await TryConnecting(this.hostApplicationLifetime.ApplicationStopping);
        }

        private async Task PushEventModel(EventModel evt, CancellationToken cancellationToken)
        {
            try
            {
                evt.UnixTimestamp = DateTimeOffset.Now.ToUniversalTime().ToUnixTimeSeconds();
                JObject obj = JObject.FromObject(evt);
                //string message = JsonConvert.SerializeObject(evt);

                string topic = "";
                MqttApplicationMessage message = CreateMessage($"hubbub/6/{evt.DeviceType}/{evt.DeviceIndex}/Event", 2, obj);
                await PublishQueue(message);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            //EventModel record = new EventModel();
            //record.UnixTimestamp = DateTimeOffset.Now.ToUniversalTime().ToUnixTimeSeconds();
            //record.DeviceType = DeviceType;
            //record.DeviceIndex = DeviceIndex;
            //this._deviceId = $"{DeviceType}/{DeviceIndex}";
            //record.SiteId = this._siteId = SiteId;
            //record.Status = status;
            //record.BitFlag = Value;
            //record.FactoryCode = FactoryCode;
            //record.GroupCode = GroupCode;
            //string message = JsonConvert.SerializeObject(record);
            //await base.PublishMessageAsync(message, token);
        }

        private async Task PublishQueue(MqttApplicationMessage queues)
        {
            if (client.IsConnected)
            {
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                await client.PublishAsync(queues);
                Console.WriteLine($"{queues.Topic} {sw.ElapsedMilliseconds} ms / {queues.Payload.Length} bytes");
            }
        }


        private async Task PublishQueue(IEnumerable<MqttApplicationMessage> queues)
        {
            if (client.IsConnected)
                await client.PublishAsync(queues);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InitializeMqtt(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //first event model push
#if EVENT
                    var evt = await globalStorage.GetEventModels(stoppingToken);
                    foreach (EventModel evtModel in evt)
                    {
                        await PushEventModel(evtModel, stoppingToken);
                    }
#else
                    //List<MqttApplicationMessage> messages = new List<MqttApplicationMessage>();
                    foreach (var pushModel in template.PushModels)
                    {
                        if (pushModel.LastReadTime < DateTime.Now)
                        {
                            if (pushModel.Items == null)
                                continue;
                            
                            foreach (var msg in pushModel.Items)
                            {
                                JObject obj = await globalStorage.BindingAndCopy(msg.Template, stoppingToken);
                                MqttApplicationMessage message = CreateMessage(msg.Topic, msg.Qos, obj);
                                //if (obj.ContainsKey("deviceId"))
                                //{
                                //    Console.WriteLine($"{msg.Topic} QOS:{msg.Qos} {obj["deviceId"]} {obj["timestamp"]}");
                                //}
                                //messages.Add(message);
                                await PublishQueue(message);
                                //await client.PublishAsync(message, stoppingToken);
                            }

                            
                            pushModel.LastReadTime = DateTime.Now.Add(pushModel.Interval);
                        }
                    }
#endif
                    //await PublishQueue(messages);
                    //logger.LogInformation("Mqtt Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(100, stoppingToken);
                }
                catch(MQTTnet.Exceptions.MqttCommunicationException mqttex)
                {
                    logger.LogError(mqttex, mqttex.Message);
                    await Task.Delay(5000, stoppingToken);
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
            }
        }

        private MqttApplicationMessage CreateMessage(string topic, int Qos, JObject message)
        {
            string msg = message.ToString();
            byte[] buffers = Encoding.UTF8.GetBytes(msg);
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)Qos)
                .WithPayload(buffers)
                .Build();
            return applicationMessage;
        }
    }
}
