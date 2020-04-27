using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using Newtonsoft.Json.Linq;
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
        private readonly IZKFactory zKFactory;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly IGlobalStorage globalStorage;
        private readonly HubbubInformation hubbubInformation;
        private readonly IMqttClient client;
        private readonly MqttClientOptions mqttClientOptions;
        private readonly Model.TemplateRoot template;

        public MqttReadWriteWorker(ILogger<MqttReadWriteWorker> logger,
            HubbubInformation hubbubInformation,
            IZKFactory zKFactory,
            IHostApplicationLifetime hostApplicationLifetime,
            IGlobalStorage globalStorage,
            MqttClientTcpOptions mqttClientTcpOptions,
            Model.TemplateRoot template)
        {
            this.logger = logger;
            this.hubbubInformation = hubbubInformation;
            this.zKFactory = zKFactory;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.globalStorage = globalStorage;
            this.template = template;

            var factory = new MqttFactory();
            this.client = factory.CreateMqttClient();
            mqttClientOptions = new MqttClientOptions
            {
                ChannelOptions = mqttClientTcpOptions
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

        public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            logger.LogInformation("### RECEIVED APPLICATION MESSAGE ###");
            logger.LogInformation($"+ Topic = {e.ApplicationMessage.Topic}");
            logger.LogInformation($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            logger.LogInformation($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            logger.LogInformation($"+ Retain = {e.ApplicationMessage.Retain}");

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
            //await client.SubscribeAsync(GetTopicFilter());
            logger.LogInformation("### SUBSCRIBED ###");
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await zKFactory.Waiting(stoppingToken);
            await InitializeMqtt(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var pushModel in template.PushModels)
                    {
                        if (pushModel.LastReadTime < DateTime.Now)
                        {
                            foreach (var msg in pushModel.Items)
                            {
                                JObject obj = await globalStorage.BindingAndCopy(msg.Template, stoppingToken);
                                MqttApplicationMessage message = CreateMessage(msg.Topic, msg.Qos, obj);
                                await client.PublishAsync(message, stoppingToken);

                            }
                            pushModel.LastReadTime = DateTime.Now.Add(pushModel.Interval);
                        }
                    }
                    logger.LogInformation("Mqtt Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(500, stoppingToken);
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
