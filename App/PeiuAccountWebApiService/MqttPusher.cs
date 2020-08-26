using Microsoft.Extensions.Configuration;
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
        const string ENV_MQTT_BINDADDRESS = "ENV_MQTT_BINDADDRESS";
        const string ENV_MQTT_PORT = "ENV_MQTT_PORT";
        public MqttPusher(ILogger<MqttPusher> logger, IConfiguration appConfig)
        {
            this.logger = logger;

            string mqttAddress = Environment.GetEnvironmentVariable(ENV_MQTT_BINDADDRESS);
            ushort mqttport = ushort.Parse(Environment.GetEnvironmentVariable(ENV_MQTT_PORT));

            mqttClientOptions = new MqttClientOptions
            {
                ChannelOptions = new MqttClientTcpOptions() { Server = mqttAddress, Port = mqttport }
            };

            var factory = new MqttFactory();
            client = factory.CreateMqttClient();
            client.DisconnectedHandler = this;
            client.ConnectedHandler = this;
            Task t = this.ConnectingMqtt();
            t.Wait();
        }

        private async Task ConnectingMqtt()
        {
            try
            {
                await client.ConnectAsync(mqttClientOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        public async Task PushAsync(JObject obj, string Topic, int Qos)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(obj.ToString());
            var applicationMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(Topic)
                        .WithPayload(buffer)
                        .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)Qos)
                        .Build();

            await client.PublishAsync(applicationMessage);
        }

        public async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            logger.LogWarning("### DISCONNECTED FROM SERVER ###");
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await ConnectingMqtt();
            }
            catch (Exception ex)
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
