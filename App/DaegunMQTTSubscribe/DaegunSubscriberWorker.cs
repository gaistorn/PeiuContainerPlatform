using FireworksFramework.Mqtt;
using MQTTnet;
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
        public DaegunSubscriberWorker(CancellationToken cancellationToken)
        {
            
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
