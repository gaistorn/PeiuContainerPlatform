using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PeiuPlatform.DataAccessor;
using NLog.Extensions.Logging;
namespace MqttToRedisWorkerApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("MAIN");
            var logger = NLog.LogManager.GetCurrentClassLogger();
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch(Exception ex)
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
                    services.AddRedisDataAccess(hostContext.Configuration);
                    services.AddHostedService<QueueReceiverWorker>();
                    services.AddHostedService<Worker>();

                });
    }
}
