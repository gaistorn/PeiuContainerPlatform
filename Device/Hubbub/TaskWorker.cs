using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.Hubbub
{
    public class TaskWorker : IHostedService, IDisposable
    {
        private readonly int id;
        private readonly ILogger<TaskWorker> _logger;
        public TaskWorker(ILogger<TaskWorker> logger,  int workerid)
        {
            id = workerid;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[{id}] Worker Start");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"[{id}] Worker Stop");
                return Task.CompletedTask;
            }
            catch(Exception ex)
            {
                return Task.CompletedTask;
            }
        }



        public  async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                try
                {
                    _logger.LogInformation($"[{id}] Worker running at: {DateTimeOffset.Now}");
                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    break;
                }
            }
        }

        public void Dispose()
        {
            _logger.LogInformation($"[{id}] Worker Dispose");
        }
    }
}
