using FireworksFramework.Mqtt;
using Microsoft.Extensions.Hosting;
using PEIU.Hubbub;
using PeiuPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{
    public class MqttHostService : BackgroundHostService
    {
        private DaegunSubscriberWorker DaegunSubscriberWorker = null;
        private readonly IPacketQueue queue;

        public MqttHostService(IPacketQueue packetQueue )
        {
            queue = packetQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DaegunSubscriberWorker = new DaegunSubscriberWorker(stoppingToken);
            DaegunSubscriberWorker.Initialize();
            DaegunSubscriberWorker.MessageReceived += DaegunSubscriberWorker_MessageReceived;
            while (stoppingToken.IsCancellationRequested == false)
            {
                await DaegunSubscriberWorker.MqttSubscribeAsync(stoppingToken);
                await Task.Delay(500);
                stoppingToken.ThrowIfCancellationRequested();
            }
        }

        

        private void DaegunSubscriberWorker_MessageReceived(object sender, MQTTnet.MqttApplicationMessageReceivedEventArgs e)
        {

            DaegunPacket packet = PacketParser.ByteToStruct<DaegunPacket>(e.ApplicationMessage.Payload);
            queue.QueueBackgroundWorkItem(packet);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
