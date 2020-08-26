using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.App
{

    public abstract class BackgroundHostService : IHostedService, IDisposable
    {
        private Task task;
        private readonly CancellationTokenSource cancellationTokens = new CancellationTokenSource();

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            this.task = this.ExecuteAsync(this.cancellationTokens.Token);

            return this.task.IsCompleted ? this.task : Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            if (this.task != null)
            {
                try
                {
                    this.cancellationTokens.Cancel();
                }
                finally
                {
                    await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cancellationToken));
                }
            }
        }

        public virtual void Dispose()
        {
            this.cancellationTokens.Cancel();
        }
    }
}
