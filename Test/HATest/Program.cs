using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet.Client.Options;

namespace HATest
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
                    IConfiguration configuration = hostContext.Configuration;
                    MqttClientTcpOptions mqttClientTcpOptions = configuration.GetSection("Mqtt").Get<MqttClientTcpOptions>();
                    services.AddSingleton(mqttClientTcpOptions);
                    services.AddHostedService<Worker>();
                });
    }
}
