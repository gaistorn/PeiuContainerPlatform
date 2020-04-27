using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransferJejuPv
{
    public class QueueReceiverWorker : BackgroundService
    {
        readonly PeiuMqttSubscriber mqttSubscriber;
        readonly ILogger<QueueReceiverWorker> _logger;
        public QueueReceiverWorker(ILogger<QueueReceiverWorker> _logger, IPacketQueue queue)
        {
            mqttSubscriber = new PeiuMqttSubscriber(_logger, queue);
            this._logger = _logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //throw new NotImplementedException();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);
            }
        }

        public override async  Task StartAsync(CancellationToken cancellationToken)
        {
            await this.mqttSubscriber.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await this.mqttSubscriber.StopAsync(cancellationToken);
            _logger.LogWarning("Stop!");
        }
    }
}
