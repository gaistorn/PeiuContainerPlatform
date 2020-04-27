using AdysTech.InfluxDB.Client.Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.DataAccessor
{
    public static class InfluxDataAccessExtension
    {
        internal const string ENV_INFLUXDB_HOST = "INFLUX_HOST";
        internal const string ENV_INFLUXDB_USERNAME = "INFLUX_USERNAME";
        internal const string ENV_INFLUXDB_PASSWORD = "INFLUX_PASSWORD";
        public static void AddInfluxDataAccess(this IServiceCollection services)
        {
            string url = Environment.GetEnvironmentVariable(ENV_INFLUXDB_HOST);
            string user = Environment.GetEnvironmentVariable(ENV_INFLUXDB_USERNAME);
            string pass = Environment.GetEnvironmentVariable(ENV_INFLUXDB_PASSWORD);
            InfluxDBClient client = new InfluxDBClient(url, user, pass);
            InfluxDataAccess access = new InfluxDataAccess(client);
            //services.AddSingleton(client);
            services.AddSingleton<IInfluxDataAccess>(access);
        }

        
    }
}
