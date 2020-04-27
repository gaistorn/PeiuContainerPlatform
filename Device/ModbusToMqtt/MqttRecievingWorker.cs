using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using Newtonsoft.Json.Linq;
using Power21.Device.Dao;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Power21.Device.Services
{
    //public interface IMqttInterface
    //{
    //    Task SubscribeAsync(CancellationToken cancellationToken);
    //    Task TryConnecting();
    //}

    public class MqttRecievingWorker : BackgroundService, 
        //IMqttInterface, 
        MQTTnet.Client.Connecting.IMqttClientConnectedHandler,
        MQTTnet.Client.Disconnecting.IMqttClientDisconnectedHandler,
        IMqttApplicationMessageReceivedHandler
    {
        readonly ILogger<MqttRecievingWorker> logger;
        readonly Map map;
        readonly IModbusInterface modbusInterface;
        readonly MqttClientTcpOptions config;
        readonly MqttClientOptions mqttClientOptions;
        readonly IMqttClient client;
        readonly IMqttQueue queue;
        readonly TimeSpan SendingQueueInterval;
        readonly PushTemplate[] PushTemplates;
        public MqttRecievingWorker(ILogger<MqttRecievingWorker> logger, Map map, IModbusInterface modbusInterface,
            IMqttQueue queue, IConfiguration config
            )
        {
            this.logger = logger;
            this.map = map;
            this.modbusInterface = modbusInterface;
            this.queue = queue;

            var factory = new MqttFactory();
            this.client = factory.CreateMqttClient();
            mqttClientOptions = new MqttClientOptions
            {
                ChannelOptions = config.GetSection("Mqtt").Get<MqttClientTcpOptions>(),
                ClientId = "CC"
               
            };
            SendingQueueInterval = config.GetSection("SendingQueueInterval").Get<TimeSpan>();
            PushTemplates = config.GetSection("SendingQueue").Get<PushTemplate[]>();
            //InitializeMqtt();
        }

        private TopicFilter[] GetTopicFilter()
        {
            List<TopicFilter> topicFilters = new List<TopicFilter>();
            
            foreach (MapControl ctl in map.Controls)
            {
                TopicFilterBuilder builder = new TopicFilterBuilder().WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce);
                builder = builder.WithTopic(ctl.Topic);
                topicFilters.Add(builder.Build());
            }
            return topicFilters.ToArray();
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
            catch(Exception ex)
            {
                logger.LogError(ex, "### RECONNECTING TO MQTT BROKER FAILED ###" + Environment.NewLine + ex.Message);
                logger.LogInformation("TRY CONNECTING AFTER 5 SECONDS");
                Thread.Sleep(TimeSpan.FromSeconds(10));
                //await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        public async Task TryConnecting()
        {
            try
            {
                await client.ConnectAsync(mqttClientOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "### RECONNECTING TO MQTT BROKER FAILED ###" + Environment.NewLine + ex.Message);
                logger.LogInformation("TRY CONNECTING AFTER 5 SECONDS");
                Thread.Sleep(TimeSpan.FromSeconds(10));
                //await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        public async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            logger.LogInformation("### CONNECTED WITH MQTT BROKER SERVER ###");
            await client.SubscribeAsync(GetTopicFilter());
            logger.LogInformation("### SUBSCRIBED ###");
        }

        public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            logger.LogInformation("### RECEIVED APPLICATION MESSAGE ###");
            logger.LogInformation($"+ Topic = {e.ApplicationMessage.Topic}");
            logger.LogInformation($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            logger.LogInformation($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            logger.LogInformation($"+ Retain = {e.ApplicationMessage.Retain}");
            
            MapControl mc =  map.Controls.FirstOrDefault(x => x.Topic == e.ApplicationMessage.Topic);
            if(mc != null)
            {
                IComparable value = mc.Point.ToValue(e.ApplicationMessage.Payload);
                queue.QueueBackgroundWorkItem((mc, value));
                logger.LogInformation($"+ Control = {mc.Point.Name}, Value = {value}");
                //e.ApplicationMessage
            }
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }

        public async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            if(eventArgs.Exception != null)
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
                await TryConnecting();
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return InitializeMqtt(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(100, stoppingToken);
            foreach (PushTemplate template in PushTemplates)
            {
                if (File.Exists(template.TemplateFile))
                {
                    Console.WriteLine($"File Exist {template.TemplateFile}");
                    template.template = JObject.Parse(File.ReadAllText(template.TemplateFile));
                }
                else
                {
                    Console.WriteLine($"File not Exist {template.TemplateFile}");
                    throw new FileNotFoundException(template.TemplateFile);
                }
                
            }
            Console.WriteLine("GoGo");
            while (!stoppingToken.IsCancellationRequested)
            {
                //logger.LogInformation("Writing Worker running at: {time}", DateTimeOffset.Now);
                foreach (PushTemplate template in PushTemplates)
                {
                    if(template.LastReadTime < DateTime.Now)
                    {
                        JObject output = await OutputFactory.Output(this.modbusInterface, template.template, stoppingToken);
                        MqttApplicationMessage message = CreateMessage(template, output);
                        await client.PublishAsync(message, stoppingToken);
                        template.LastReadTime = DateTime.Now.Add(template.PushInterval);
                        logger.LogInformation(output.ToString());
                    }
                }
                
                //client.PublishAsync()
                await Task.Delay(100, stoppingToken);
            }
        }

        private MqttApplicationMessage CreateMessage(PushTemplate template, JObject message)
        {
            string msg = message.ToString();
            byte[] buffers = Encoding.UTF8.GetBytes(msg);
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(template.Topic)
                .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)template.QosLevel)
                .WithPayload(buffers)
                .Build();
            return applicationMessage;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
