using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PeiuPlatform.App
{
    public class Program
    {
        public static readonly string Secret;
        public const string ENV_JWT_SECRET = "JWT_SECRET";
        static Program()
        {
            Secret = Environment.GetEnvironmentVariable(ENV_JWT_SECRET);
        }
    
        public static void Main(string[] args)
            {
                CreateHostBuilder(args).Build().Run();
            }

            public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseUrls("https://*:443");
                        webBuilder.UseKestrel();
                        webBuilder.UseStartup<Startup>();
                    });
        }
}
