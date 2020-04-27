using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
namespace Power21.Device
{
    public class Program
    {
        static CancellationTokenSource cts;
        public static void Main(string[] args)
        {
            cts = new CancellationTokenSource();
            Console.CancelKeyPress += Console_CancelKeyPress;
            if(ArgumentParser.TryParse(args, out ArgumentParser parser))
            {
                HeartbeatFactory.AwatingActiveServer(parser, cts.Token);
            }
            else
            {
                ArgumentParser.PrintHelp();
                return;
            }

            if (cts.IsCancellationRequested)
                return;

            CreateHostBuilder(args).Build().Run();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            cts.Cancel();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
            .ConfigureServices(services =>
            {
                if (ArgumentParser.TryParse(args, out ArgumentParser parser))
                {
                    services.AddSingleton(parser);
                }
            });
    }
}
