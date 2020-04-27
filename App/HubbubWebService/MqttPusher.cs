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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public interface IMqttPusher
    {
        Task PushAsync(JObject obj, string Topic, int Qos);
    }

    public class MqttPusher : 
        IMqttPusher,
        MQTTnet.Client.Disconnecting.IMqttClientDisconnectedHandler,
        MQTTnet.Client.Connecting.IMqttClientConnectedHandler
    {
        private readonly ILogger<MqttPusher> logger;
        private readonly MqttClientOptions mqttClientOptions;
        private readonly IMqttClient client;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        public MqttPusher(ILogger<MqttPusher> logger, IHostApplicationLifetime hostApplicationLifetime)
        {
            this.logger = logger;
            this.hostApplicationLifetime = hostApplicationLifetime;
            mqttClientOptions = new MqttClientOptions
            {
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = Environment.GetEnvironmentVariable("MQTT_HOST"),
                    Port = int.Parse(Environment.GetEnvironmentVariable("MQTT_PORT"))
                }
            };

            var factory = new MqttFactory();
            client = factory.CreateMqttClient();
            client.DisconnectedHandler = this;
            client.ConnectedHandler = this;
            Task t = this.ConnectingMqtt(hostApplicationLifetime.ApplicationStopping);
            t.Wait();
        }

        private async Task ConnectingMqtt(CancellationToken cancellationToken)
        {
            try
            {
                await client.ConnectAsync(mqttClientOptions, cancellationToken);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        public async Task PushAsync(JObject obj, string Topic, int Qos)
        {
            await PushAsync(obj, Topic, Qos, hostApplicationLifetime.ApplicationStopping);
        }

        public async Task PushAsync(JObject obj, string Topic, int Qos, CancellationToken token)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(obj.ToString());
            var applicationMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(Topic)
                        .WithPayload(buffer)
                        .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)Qos)
                        .Build();

            await client.PublishAsync(applicationMessage, token);
        }

        public async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            logger.LogWarning("### DISCONNECTED FROM SERVER ###");
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await ConnectingMqtt(hostApplicationLifetime.ApplicationStopping);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "### RECONNECTING FAILED ###" + "\r\n" + ex.Message);
            }
        }

        public Task HandleConnectedAsync(MqttClientConnectedEventArgs e)
        {
            logger.LogInformation("### CONNECTED WITH SERVER ###");
            return Task.CompletedTask;
            //await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("#").Build());

        }
    }
}
