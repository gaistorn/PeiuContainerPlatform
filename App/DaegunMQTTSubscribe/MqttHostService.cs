using FireworksFramework.Mqtt;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        readonly ILogger<MqttHostService> logger;

        public MqttHostService(IPacketQueue packetQueue, ILogger<MqttHostService> logger)
        {
            queue = packetQueue;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DaegunSubscriberWorker = new DaegunSubscriberWorker(stoppingToken);
            DaegunSubscriberWorker.Initialize();
            DaegunSubscriberWorker.MessageReceived += DaegunSubscriberWorker_MessageReceived;
            while (stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    await DaegunSubscriberWorker.MqttSubscribeAsync(stoppingToken);
                    await Task.Delay(500);
                    stoppingToken.ThrowIfCancellationRequested();
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
            }
        }

        

        private void DaegunSubscriberWorker_MessageReceived(object sender, MQTTnet.MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                DaegunPacket packet = PacketParser.ByteToStruct<DaegunPacket>(e.ApplicationMessage.Payload);
                queue.QueueBackgroundWorkItem(packet);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
