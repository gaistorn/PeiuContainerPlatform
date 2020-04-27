using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PeiuPlatform.App
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        readonly EventSubscribeWorker eventSubscribeWorker;

        public Worker(ILogger<Worker> logger, EventSubscribeWorker eventSubscribe)
        {
            _logger = logger;
            eventSubscribeWorker = eventSubscribe;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await eventSubscribeWorker.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await eventSubscribeWorker.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(100, stoppingToken);
            }
        }
    }
}
