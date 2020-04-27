using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeiuPlatform.Hubbub
{
    public abstract class Tasker : IHostedService,IDisposable
    {
        public void Terminate()
        {

        }

        public abstract Task StartAsync(CancellationToken cancellationToken);

        public abstract Task ExecuteAsync(CancellationToken cancellationToken);

        public abstract Task StopAsync(CancellationToken cancellationToken);

        public void Dispose()
        {
            Task[] tasks;
            
        }
    }
}
