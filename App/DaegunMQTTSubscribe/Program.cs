using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FireworksFramework.Mqtt;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Web;
namespace PeiuPlatform.App
{
    public class Program
    {
        public static NLog.Logger NLogger = null;
        public static void Main(string[] args)
        {
            // NLog: setup the logger first to catch all errors
            AbsMqttBase.SetDefaultLoggerName("nlog.config", true);
            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                
                logger.Error("DEBUG~ INIT");
                //LogFactory = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config");
                //var logger = LogFactory.GetCurrentClassLogger();
                //logger.Info("start");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch(Exception ex)
            {
                logger.Error(ex.Message + "\n" + ex.StackTrace);
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                 //.UseKestrel()
                 .UseUrls("http://*:16000")
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Trace);
        }).UseNLog();
    }
}
