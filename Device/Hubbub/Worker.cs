using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PeiuPlatform.Hubbub;

namespace Hubbub
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly TaskWorker[] Workers;
        private readonly IHostApplicationLifetime applicationLifetime;

        public Worker(ILogger<Worker> logger, ILogger<TaskWorker> taskWorkLogger, IHostApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            this.applicationLifetime = applicationLifetime;
            Workers = new TaskWorker[10];
            for (int i=0;i<10;i++)
            {
                Workers[i] = new TaskWorker(taskWorkLogger, i);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Task.WaitAll(Workers.Select(x => x.StartAsync(applicationLifetime.ApplicationStarted)).ToArray());
            return base.StartAsync(cancellationToken);
        }

        //private async Task Working(int id, CancellationToken cancellationToken)
        //{
        //    while (!cancellationToken.IsCancellationRequested)
        //    {
        //        _logger.LogInformation($"[{id}] Worker running at: {DateTimeOffset.Now}");
        //        await Task.Delay(1000, cancellationToken);
        //    }
        //}

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Task.WaitAll(Workers.Select(x => x.StopAsync(applicationLifetime.ApplicationStopping)).ToArray());
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Task.WaitAll(Workers.Select(x => x.ExecuteAsync(applicationLifetime.ApplicationStopping)).ToArray());
           
            try
            {
                var cert = new X509Certificate2("www_peiu_co_kr.pfx", "61072");
                var httpClientHandler = new HttpClientHandler();
                // Return `true` to allow certificates that are untrusted/invalid
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                var httpClient = new HttpClient(httpClientHandler);
                
                SslCredentials secureChannel = new SslCredentials(File.ReadAllText("www_peiu_co_kr.crt"));
                httpClientHandler.ClientCertificates.Add(cert);
                GrpcChannelOptions opt = new GrpcChannelOptions();
                opt.Credentials = secureChannel;
                opt.HttpClient = httpClient;

                var channel = GrpcChannel.ForAddress("https://www.peiu.co.kr:3026", opt);

                var svc = new PeiuPlatform.Proto.HubbubService.HubbubServiceClient(channel);
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = await svc.GetAllModbusHubbubsBySiteIdAsync(new PeiuPlatform.Proto.RequestSiteid { Siteid = 160 });
                    foreach (var row in result.Modbushubbubs)
                    {
                        Console.WriteLine($"{row.Siteid} {row.Host}");
                    }

                    var point_result = await svc.GetAllModbusDataPointsAsync(new PeiuPlatform.Proto.RequestProtocolId { Protocolid = 1 });

                    _logger.LogInformation($"[MAIN] Worker running at: {DateTimeOffset.Now}");
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
