using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace PongServer
{
    public class DummyWorker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public DummyWorker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("......");
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ServerAsync(CancellationToken stoppingToken)
        {
            using (var responseSocket = new ResponseSocket("@tcp://*:5000"))
            {
                var (frame, more) = await responseSocket.ReceiveFrameBytesAsync(stoppingToken);
                if (frame.SequenceEqual(PingPong.PING_SIGNAL_FLAG))
                {
                    responseSocket.SendFrame(PingPong.PONG_SIGNAL_FLAG);
                    Console.WriteLine("PONG");
                }
            }
        }
    }
}
