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
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private ushort PubPort = 12345;
        private readonly string LookupServer;
        private static bool RunState = false;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        public Worker(ILogger<Worker> logger, ExecuteArguments args, IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            PubPort = args.HostPort;
            LookupServer = args.LookupServerAddress;
            this.hostApplicationLifetime = hostApplicationLifetime;


        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        private async Task PubSync(CancellationToken cancellationToken)
        {
            while(RunState == false)
            {
                await Task.Delay(100, hostApplicationLifetime.ApplicationStopping);
                if (hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                    return;
            }
            Random rand = new Random(50);
            using (var pubSocket = new PublisherSocket())
            {
                pubSocket.Options.SendHighWatermark = 1000;
                pubSocket.Bind($"tcp://*:{PubPort}");

                int idx = 0;
                while (!hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    idx++;
                    var randomizedTopic = rand.NextDouble();
                    string topic = "TopicA/" + idx;
                    if (randomizedTopic > 0.5)
                    {
                        topic = "TopicB/" + idx;
                    }
                    Console.WriteLine($"[{topic}] Sending message : {idx}");
                    pubSocket.SendMoreFrame(topic).SendFrame(BitConverter.GetBytes(idx));
                    await Task.Delay(1000, hostApplicationLifetime.ApplicationStopping);
                }
            }
        }

        private async Task SubAsync(CancellationToken cancellationToken)
        {
            using(var subSocket = new SubscriberSocket())
            {
                subSocket.Options.ReceiveHighWatermark = 1000;
                subSocket.Connect(LookupServer);
                subSocket.Subscribe("Topic");
                Console.WriteLine("Subscriber socket connecting...");
                DateTime dt = DateTime.Now;
                while (!hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    if (subSocket.TryReceiveFrameString(TimeSpan.FromSeconds(10), out string topic))
                    {
                        (byte[] buffer, bool IsOk) result = await subSocket.ReceiveFrameBytesAsync(hostApplicationLifetime.ApplicationStopping);
                        int idx = BitConverter.ToInt32(result.buffer, 0);
                        Console.WriteLine("...");
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("MODE CHANGE => ACTIVE...");
                        RunState = true;
                        break;

                    }
                }
                //(string msg, bool isOK) result = await subSocket.ReceiveFrameStringAsync(cancellationToken);
                //Console.WriteLine("Received Message: " + result.msg);
            }
        }



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //using(var runTime = new NetMQRuntime())
            //{
            //    //runTime.Run(SubAsync(stoppingToken), PubSync(stoppingToken));
            //}

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("[V2] Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }

        }        
    }
}
