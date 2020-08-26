using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

namespace PeiuPlatform.Hubbub
{
    public static class ServiceInitializer
    {
        public static void Configuration(string[] args, IConfiguration configuration, IServiceCollection services)
        {
            try
            {

                BuildLogger(services, configuration);
                LoadConfiguration(configuration, services);
                RegistrationServices(services);
            }
            catch(Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex, ex.Message);
            }
        }

        #region #Logic#

        private static void BuildLogger(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(logger =>
            {
                logger.ClearProviders();
                logger.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                logger.AddNLog(configuration);
            });
        }

        private static void LoadConfiguration(IConfiguration configuration, IServiceCollection services)
        {
            HubbubInformation hubbubConfig = configuration.GetSection("hubbub").Get<HubbubInformation>();
            services.AddSingleton(hubbubConfig);
            var redisConfiguration = configuration.GetSection("redis").Get<RedisConfiguration>();
            ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfiguration.ConfigurationOptions);
            services.AddSingleton(connectionMultiplexer);

            string etridb_connstr = configuration.GetConnectionString("etridb");
            MysqlDataAccess mysqlDataAccess = new MysqlDataAccess(etridb_connstr);
            services.AddSingleton(mysqlDataAccess);
        }

        private static void RegistrationServices(IServiceCollection services)
        {
            services.AddSingleton<IRedisStorage, RedisStorage>();
            services.AddHostedService<Worker>();
        }
        #endregion
    }
}
