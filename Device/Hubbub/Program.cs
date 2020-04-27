using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hubbub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //for (int i = 0; i < 3; i++)
                    //{
                    //    services.AddHostedService(
                    //        serviceprovider => new Worker(
                    //           serviceprovider.GetService<ILogger<Worker>>(),
                    //           i)
                    //   );
                    //}
                    services.AddHostedService<Worker>();
                });
    }
}
