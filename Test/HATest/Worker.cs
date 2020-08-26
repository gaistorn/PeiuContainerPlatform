using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;

namespace HATest
{
    public class Worker : BackgroundService,
         MQTTnet.Client.Connecting.IMqttClientConnectedHandler,
        MQTTnet.Client.Disconnecting.IMqttClientDisconnectedHandler
    {
        private readonly ILogger<Worker> logger;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly IMqttClient client;
        private readonly MqttClientOptions mqttClientOptions;

        public Worker(ILogger<Worker> logger,
            IHostApplicationLifetime hostApplicationLifetime,
            MqttClientTcpOptions mqttClientTcpOptions)
        {
            this.logger = logger;
            this.hostApplicationLifetime = hostApplicationLifetime;

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

       
        public async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            logger.LogInformation("### CONNECTED WITH MQTT BROKER SERVER ###");
#if !EVENT
            //await client.SubscribeAsync(CreateTopicFilter());

            logger.LogInformation("### SUBSCRIBED ###");
#endif
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
            string hostName = Dns.GetHostName();
            string topic = $"ha/{hostName}";
            Console.WriteLine("Publish Topic: " + topic);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //report.strHost = Dns.GetHostName();
                    //report.strIp = Dns.GetHostEntry(report.strHost).AddressList[0].ToString();
                    //report.strOSVer = Environment.OSVersion.ToString();
                    
                    string output = $"[{DateTimeOffset.Now}] Host:{hostName}\nAddr: {string.Join('\n', Dns.GetHostEntry(hostName).AddressList.Select(x=>x.ToString()))}\nOS: {Environment.OSVersion}";

                    
                    MqttApplicationMessage message = CreateMessage(topic, 0, output);
                    await PublishQueue(message);
                    logger.LogInformation("Mqtt Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);
                }
                catch (MQTTnet.Exceptions.MqttCommunicationException mqttex)
                {
                    logger.LogError(mqttex, mqttex.Message);
                    await Task.Delay(5000, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
            }
        }

        private MqttApplicationMessage CreateMessage(string topic, int Qos, string message)
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
