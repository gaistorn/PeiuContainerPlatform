using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PeiuPlatform.App;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.Hubbub
{
    public class EtriDataWriteWorker : BackgroundService
    {
        private readonly ILogger<EtriDataWriteWorker> _logger;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly IGlobalStorage<EventModel> globalStorage;
        private readonly HubbubInformation hubbubInformation;

        public EtriDataWriteWorker(ILogger<EtriDataWriteWorker> logger,
            HubbubInformation hubbubInformation,
            IHostApplicationLifetime hostApplicationLifetime,
            IGlobalStorage<EventModel> globalStorage)
        {
            _logger = logger;
            this.hubbubInformation = hubbubInformation;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.globalStorage = globalStorage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //CancellationToken cancelToken = hostApplicationLifetime.ApplicationStopping;

            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                try
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
                await Task.Delay(hubbubInformation.ScanRateMS, stoppingToken);
            }
        }
                
    }
}
