using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions;
using NLog.Extensions.Logging;

namespace PeiuPlatform
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private void ReadMembers(object obj, string Alias, IDictionary<object, object> properties)
        {
            foreach(PropertyInfo property in obj.GetType().GetProperties())
            {
                if(property.PropertyType.IsValueType && property.PropertyType.IsEnum == false)
                {
                    ReadMembers(obj, string.IsNullOrEmpty(Alias)
                        ? property.Name
                        : Alias + "." + property.Name, properties);
                }
                else if(properties.ContainsKey(property.Name) == false)
                {
                    properties.Add(string.IsNullOrEmpty(Alias) 
                        ? property.Name
                        : Alias + "." + property.Name, property.GetValue(obj));
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IPacketSubscriber, DataSubscriberWorker>();
                    services.AddHostedService<DataPublisherWorker>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddNLog();
                });
    }
}
