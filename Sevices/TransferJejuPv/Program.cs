using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PeiuPlatform.DataAccessor;
using Microsoft.Extensions.Configuration;
using NLog.Extensions.Logging;
namespace TransferJejuPv
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

                    string mqtt_ip = hostContext.Configuration.GetSection("MQTT:ENV_MQTT_BINDADDRESS").Get<string>();
                    ushort mqtt_port = hostContext.Configuration.GetSection("MQTT:ENV_MQTT_PORT").Get<ushort>(); 
                    string mqtt_topic = hostContext.Configuration.GetSection("MQTT:ENV_MQTT_TOPIC").Get<string>();

                    Environment.SetEnvironmentVariable("ENV_MQTT_BINDADDRESS", mqtt_ip);
                    Environment.SetEnvironmentVariable("ENV_MQTT_PORT", mqtt_port.ToString());
                    Environment.SetEnvironmentVariable("ENV_MQTT_TOPIC", mqtt_topic);

                    var redisConfiguration = hostContext.Configuration.GetSection("TargetUdpHost").Get<UdpHost>();

                    services.AddSingleton(redisConfiguration);
                    services.AddSingleton<IPacketQueue, PacketQueue>();
                    services.AddHostedService<QueueReceiverWorker>();
                    services.AddHostedService<Worker>();

                });
    }
}
