using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PeiuPlatform.DataAccessor;
using NLog.Extensions.Logging;
using PeiuPlatform.Notification;

namespace PeiuPlatform.App
{
    public class Program
    {
        static void Main(string[] args)
        {
            //Notificator.API_KEY = Environment.GetEnvironmentVariable("TOAST_API_KEY");
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
                    services.AddEventDataAccessor();
                    services.AddRedisDataAccess(hostContext.Configuration);

                    Notificator notificator = new Notificator(Environment.GetEnvironmentVariable("TOAST_API_KEY"));
                    var logger = NLog.LogManager.GetCurrentClassLogger();
                    notificator.Logger = logger;
                    services.AddSingleton(notificator);
                    services.AddSingleton<EventSubscribeWorker>();
                    services.AddHostedService<Worker>();

                });
    }
}
