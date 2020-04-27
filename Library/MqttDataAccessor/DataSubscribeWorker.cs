using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PeiuPlatform.DataAccessor
{
    public delegate Task ApplicationMessageReceivedHandlerDelegate(string ClientId, string Topic, string ContentType, uint QosLevel, byte[] payload);
    public delegate Task ClientDisconnectedHandlerDelegate(bool ClientWasConnected, Exception ex);
    public delegate Task ClientConnectedHandlerDelegate();
    public abstract class DataSubscribeWorker : IDisposable
    {
        public event ApplicationMessageReceivedHandlerDelegate ApplicationMessageReceived;
        public event ClientDisconnectedHandlerDelegate ClientDisconnected;
        public event ClientConnectedHandlerDelegate ClientConnected;
        public string BindAddress { get; private set; }
        public ushort Port { get; private set; }
        public string Topic { get; private set; }
        public ushort QoS { get; set; }
        public bool AutoConnectWhenDisconnect { get; set; } = true;

        private IMqttClient mqtt_client;

        const string ENV_MQTT_BINDADDRESS = "ENV_MQTT_BINDADDRESS";
        const string ENV_MQTT_PORT = "ENV_MQTT_PORT";
        const string ENV_MQTT_TOPIC = "ENV_MQTT_TOPIC";

        public async Task<bool> ConnectionAsync()
        {
            string bindAddress = Environment.GetEnvironmentVariable(ENV_MQTT_BINDADDRESS);
            string portString = Environment.GetEnvironmentVariable(ENV_MQTT_PORT);
            string topic = Environment.GetEnvironmentVariable(ENV_MQTT_TOPIC);
            ushort port = ushort.Parse(portString);
            return await ConnectionAsync(bindAddress, port, 0, topic);
        }

        public async Task<bool> ConnectionAsync(string bindAddress, ushort port, ushort QoSLevel, string SubscribeTopic)
        {
            this.BindAddress = bindAddress;
            this.Port = port;
            QoS = QoSLevel;
            Topic = SubscribeTopic;

            return await TryConnecting();
            //    [JsonProperty("ClientId")]
            //public string ClientId { get; set; }

            //[JsonProperty("BindAddress")]
            //public string BindAddress { get; set; }

            //[JsonProperty("Port")]
            //public ushort Port { get; set; } = 1883;

            //[JsonProperty("QosLevel")]
            //public int QosLevel { get; set; }
        }

        private MqttClientOptions CreateMqttOption()
        {
            var ClientOptions = new MqttClientOptions
            {
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = this.BindAddress,
                    Port = this.Port,

                }


            };
            return ClientOptions;
        }

        protected abstract Task OnApplicationMessageReceived(string ClientId, string Topic, string ContentType, uint QosLevel, byte[] payload);
        protected virtual Task OnConnected()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnDisconnected(bool ClientWasConnected, Exception exception)
        {
            return Task.CompletedTask;
        }

        private async Task ManagedClient_ApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {

            await OnApplicationMessageReceived(e.ClientId, e.ApplicationMessage.Topic, e.ApplicationMessage.ContentType, (uint)e.ApplicationMessage.QualityOfServiceLevel, e.ApplicationMessage.Payload);
            if (ApplicationMessageReceived != null)
            {
                await ApplicationMessageReceived.Invoke(e.ClientId, e.ApplicationMessage.Topic, e.ApplicationMessage.ContentType, (uint)e.ApplicationMessage.QualityOfServiceLevel, e.ApplicationMessage.Payload);
            }
        }

        public void RunSubscribing()
        {
            mqtt_client.SubscribeAsync();
        }

        private async Task ManagedClient_Connected(MqttClientConnectedEventArgs e)
        {
            await mqtt_client.SubscribeAsync(new TopicFilterBuilder().WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)QoS).WithTopic(Topic).Build());
            if (ClientConnected != null)
                await ClientConnected.Invoke();
            await OnConnected();
        }

        private async Task ManagedClient_Disconnected(MqttClientDisconnectedEventArgs e)
        {
            if (ClientDisconnected != null)
                await (ClientDisconnected.Invoke(e.ClientWasConnected, e.Exception));

            await OnDisconnected(e.ClientWasConnected, e.Exception);

            if (AutoConnectWhenDisconnect)
                await mqtt_client.ReconnectAsync();


        }

        protected async Task<bool> TryConnecting()
        {
            MqttClientOptions options = CreateMqttOption();
            if (mqtt_client == null)
            {
                mqtt_client = new MqttFactory().CreateMqttClient();
                mqtt_client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(ManagedClient_ApplicationMessageReceived);
                mqtt_client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(ManagedClient_Connected);
                mqtt_client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(ManagedClient_Disconnected);
            }


            var result = await mqtt_client.ConnectAsync(options);

            return result.ResultCode == MqttClientConnectResultCode.Success;
        }

        protected virtual void OnDisposing()
        {

        }

        public void Dispose()
        {
            if (mqtt_client != null)
                mqtt_client.Dispose();
            OnDisposing();
        }
    }
}
