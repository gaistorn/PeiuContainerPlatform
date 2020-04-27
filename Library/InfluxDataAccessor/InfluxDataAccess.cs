using AdysTech.InfluxDB.Client.Net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using static PeiuPlatform.DataAccessor.InfluxDataAccessExtension;

namespace PeiuPlatform.DataAccessor
{
    public interface IInfluxDataAccess
    {
        Task<IReadOnlyList<dynamic>> Query(string database, string measurements, int siteid, DateTime start, DateTime end, string add_where_clause = null);

        Task<double> Sum(string database, string measurements, string fieldName, int siteid, DateTime start, DateTime end, string add_where_clause = null);
        Task<double> Average(string database, string measurements, string fieldName, int siteid, DateTime start, DateTime end, string add_where_clause = null);

        long ToUnixTimeSeconds(DateTime dt);
    }
    public class InfluxDataAccess : IInfluxDataAccess
    {
        private readonly InfluxDBClient influxDB;
        public InfluxDataAccess(InfluxDBClient client)
        {
            this.influxDB = client;
        }

        public static InfluxDataAccess CreateDataAccessFromEnvironment()
        {
            string url = Environment.GetEnvironmentVariable(ENV_INFLUXDB_HOST);
            string user = Environment.GetEnvironmentVariable(ENV_INFLUXDB_USERNAME);
            string pass = Environment.GetEnvironmentVariable(ENV_INFLUXDB_PASSWORD);
            var influxDB = new InfluxDBClient(url, user, pass);
            InfluxDataAccess access = new InfluxDataAccess(influxDB);
            return access;
        }

        public long ToUnixTimeSeconds(DateTime dt)
        {
            DateTime utcTime = dt.ToUniversalTime(); //GMT 시각으로 변경해야함
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            long unixTime = (utcTime.Ticks - epochTicks) / TimeSpan.TicksPerSecond;
            return unixTime;
        }

        public async Task<IReadOnlyList<dynamic>> Query(string database, string measurements, int siteid, DateTime start, DateTime end, string add_where_clause = null)
        {
            long ULstart = ToUnixTimeSeconds(start);
            long ULend = ToUnixTimeSeconds(end);
            return await Query(database, measurements, siteid, ULstart, ULend, add_where_clause);
        }

        public async Task<double> Sum(string database, string measurements, string fieldName, int siteid, DateTime start, DateTime end, string add_where_clause = null)
        {
            long ULstart = ToUnixTimeSeconds(start);
            long ULend = ToUnixTimeSeconds(end);
            string query = $"select sum({fieldName}) from {measurements} where siteId = '{siteid}' and time >= {ULstart}s and time <= {ULend}s";
            if (add_where_clause != null)
                query = query + " and " + add_where_clause;
            Console.WriteLine("INFLUX QUERY: " + query);
            var result = await influxDB.QueryMultiSeriesAsync(database, query);
            if (result.Count > 0 && result[0].Entries.Count > 0)
            {
                return Convert.ToDouble(result[0].Entries[0].Sum);
            }
            else
                return 0;
        }

        public async Task<double> Average(string database, string measurements, string fieldName, int siteid, DateTime start, DateTime end, string add_where_clause = null)
        {
            long ULstart = ToUnixTimeSeconds(start);
            long ULend = ToUnixTimeSeconds(end);
            string query = $"select mean({fieldName}) from {measurements} where siteId = '{siteid}' and time >= {ULstart}s and time <= {ULend}s";
            if (add_where_clause != null)
                query = query + " and " + add_where_clause;
            Console.WriteLine("INFLUX QUERY: " + query);
            var result = await influxDB.QueryMultiSeriesAsync(database, query);
            if (result.Count > 0 && result[0].Entries.Count > 0)
            {
                return Convert.ToDouble(result[0].Entries[0].Mean);
            }
            else
                return 0;
        }

        private async Task<IReadOnlyList<dynamic>> Query(string database, string measurements, int siteid, long start, long end, string add_where_clause = null)
        {

            string query = $"select * from {measurements} where siteId = '{siteid}' and time >= {start}s and time <= {end}s";
            if (add_where_clause != null)
                query = query + " and " + add_where_clause;
            var result = await influxDB.QueryMultiSeriesAsync(database, query);
            return result[0].Entries;
        }
    }
}
