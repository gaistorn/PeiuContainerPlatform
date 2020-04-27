using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Options;
using NetMQ.Sockets;
using NetMQ;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Power21.Device.Dao;
using Power21.Device.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Power21.Device
{
    public class HeartBeatWorker : BackgroundService
    {
        private readonly ILogger<HeartBeatWorker> logger;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly ArgumentParser argument;
        public static readonly byte[] PING_SIGNAL_FLAG = { 0x01, 0x09, 0x08, 0x03 };
        public static readonly byte[] PONG_SIGNAL_FLAG = { 0x00, 0x03, 0x00, 0x08 };
        public HeartBeatWorker(ILogger<HeartBeatWorker> logger, ArgumentParser argument, IHostApplicationLifetime hostApplicationLifetime)
        {
            this.logger = logger;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.argument = argument;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(100, stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var runtime = new NetMQRuntime())
                {
                    runtime.Run(hostApplicationLifetime.ApplicationStopping, ServerAsync(hostApplicationLifetime.ApplicationStopping));
                }

            }
        }

        private async Task ServerAsync(CancellationToken stoppingToken)
        {
            using (var pubSocket = new PublisherSocket())
            {
                pubSocket.Options.SendHighWatermark = 1000;
                pubSocket.Bind($"tcp://*:{argument.HeartBeatServerPort}");

                int idx = 0;
                while (!stoppingToken.IsCancellationRequested)
                {
                    string topic = "hubbub";
                    pubSocket.SendMoreFrame(topic).SendFrame(PING_SIGNAL_FLAG);
                    await Task.Delay(1000, hostApplicationLifetime.ApplicationStopping);
                }
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Writing Worker stoping at: {time}", DateTimeOffset.Now);
            return base.StopAsync(cancellationToken);
        }
    }
}
