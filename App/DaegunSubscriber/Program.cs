using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FireworksFramework.Mqtt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using PeiuPlatform.App;
using PeiuPlatform.Lib;

namespace DaegunSubscriber
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AbsMqttBase.SetDefaultLoggerName("nlog.config", true);
            var logger = NLog.LogManager.GetCurrentClassLogger();
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(log =>
                    {
                        log.ClearProviders();
                        log.SetMinimumLevel(LogLevel.Trace);
                        log.AddNLog(hostContext.Configuration);
                    });
                    services.AddSingleton<IPacketQueue, PacketQueue>();

                    EventPublisherWorker eventPublisher = new EventPublisherWorker(2);
                    eventPublisher.Initialize();
                    services.AddSingleton(eventPublisher);
                    services.AddSingleton<DaegunSubscriberWorker>();
                    services.AddHostedService<Worker>();
                });
    }
}
