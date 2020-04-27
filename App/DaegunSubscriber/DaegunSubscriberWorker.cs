using FireworksFramework.Mqtt;
using MQTTnet;
using PeiuPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public class DaegunSubscriberWorker : AbsMqttSubscriber
    {
        private PeiuPublishWorker PeiuPublishWorker;

        readonly CancellationToken token;
        readonly IPacketQueue queue;
        public DaegunSubscriberWorker(IPacketQueue packetQueue)
        {
            this.queue = packetQueue;
            this.Initialize();
        }

        

        protected override Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            DaegunPacket packet = PacketParser.ByteToStruct<DaegunPacket>(e.ApplicationMessage.Payload);
            queue.QueueBackgroundWorkItem(packet);
            return Task.CompletedTask;
        }

        //protected override async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        //{
        //    if (mqttMessageFormat != MessageFormats.JSON)
        //        throw new NotSupportedException("only supported json formats");

        //    DaegunPacket packet = PacketParser.ByteToStruct<DaegunPacket>(e.ApplicationMessage.Payload);
        //    await PublishAsync(packet);
        //}
    }
}
