using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IO;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using PeiuPlatform.App;
using System.Collections.Concurrent;

namespace PeiuPlatform.Hubbub
{
    public static class ServiceInitializer
    {
        public static void Configuration(string[] args, IConfiguration configuration, IServiceCollection services)
        {
            try
            {
                BuildLogger(services, configuration);
                LoadModels(services);
                LoadConfiguration(configuration, services);
                RegistrationServices(services);
            }
            catch(Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex, ex.Message);
            }
        }

        #region #Logic#
        private static void LoadModels(IServiceCollection services)
        {
            Model.TemplateRoot root = new Model.TemplateRoot();
            string mappingFileFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PushTemplate");
            root.PushModels = JsonFileReader.OpenFileInFolder<Model.PushModel>(mappingFileFolder);
            services.AddSingleton(root);
        }

        private static void LoadConfiguration(IConfiguration configuration, IServiceCollection services)
        {
            HubbubInformation hubbubConfig = configuration.GetSection("hubbub").Get<HubbubInformation>();
            MqttClientTcpOptions mqttClientTcpOptions = configuration.GetSection("Mqtt").Get<MqttClientTcpOptions>();
            var redisConfiguration = configuration.GetSection("redis").Get<RedisConfiguration>();
            ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions);
            services.AddSingleton(connectionMultiplexer);
            services.AddSingleton(mqttClientTcpOptions);
            services.AddSingleton(hubbubConfig);
        }

        private static void BuildLogger(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(logger =>
            {
                logger.ClearProviders();
                logger.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                logger.AddNLog(configuration);
            });
        }

        private static void RegistrationServices(IServiceCollection services)
        {
            services.AddSingleton<IGlobalStorage<EventModel>, RedisStorage>();
            services.AddHostedService<MqttReadWriteWorker>();
            services.AddHostedService<ModbusReadWriteWorker>();
        }
        #endregion
    }
}
