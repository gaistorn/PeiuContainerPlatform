using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis.Extensions.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.DataAccessor
{
    public static class RedisDataAccessorExtension
    {
        public static void AddRedisDataAccess(this IServiceCollection services, IConfiguration configuration, string RedisConfigSectionName = "redis")
        {
            var redisConfiguration = configuration.GetSection(RedisConfigSectionName).Get<RedisConfiguration>();
            services.AddSingleton(redisConfiguration);
            services.AddSingleton<IRedisDataAccessor, RedisDataAccessor>();
        }

        //public static void AddRedisDataAccess(this IServiceCollection services)
        //{
        //    services.AddSingleton<IRedisDataAccessor, RedisDataAccessor>();
        //}
    }
}
