using AdysTech.InfluxDB.Client.Net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using PeiuPlatform.Models;
using PeiuPlatform.Models.Mysql;
using PeiuPlatform.DataAccessor;

namespace PeiuPlatform.App
{
    class Program
    {
        const string ENV_MYSQL_USERNAME = "MYSQL_USERNAME";
        const string ENV_MYSQL_PASSWORD = "MYSQL_PASSWORD";
        const string ENV_MYSQL_HOST = "MYSQL_HOST";
        const string ENV_MYSQL_PORT = "MYSQL_PORT";
        const string ENV_MYSQL_DATABASE = "MYSQL_DATABASE";
        const string ENV_INFLUXDB_HOST = "INFLUX_HOST";
        const string ENV_INFLUXDB_USERNAME = "INFLUX_USERNAME";
        const string ENV_INFLUXDB_PASSWORD = "INFLUX_PASSWORD";

        static readonly InfluxDBClient influxDBClient;
        static MysqlDataAccessor dataAccessor;
        static NLog.LogFactory logFactory;
        static NLog.ILogger logger;
        static Program()
        {
            string mysql_conn = "";
            logFactory = NLog.LogManager.LoadConfiguration("nlog.config");
            logger = logFactory.GetLogger("CumulateHourlyApp");
#if DEBUG
            Console.WriteLine("DEBUG MODE");
            string influx_hostname = "http://192.168.0.40:8086";
            string influx_username = "power21";
            string influx_password = "power211234/";
            
            
            mysql_conn = PeiuPlatform.DataAccessor.DataAccessorBase.CreateConnectionString("192.168.0.40", "3306", "power21", "123qwe");
            dataAccessor = new MysqlDataAccessor(mysql_conn);
#else
            string influx_hostname = Environment.GetEnvironmentVariable(ENV_INFLUXDB_HOST);
            string influx_username = Environment.GetEnvironmentVariable(ENV_INFLUXDB_USERNAME);
            string influx_password = Environment.GetEnvironmentVariable(ENV_INFLUXDB_PASSWORD);

            dataAccessor = MysqlDataAccessor.CreateDataAccessFromEnvironment();
#endif
            influxDBClient = new InfluxDBClient(influx_hostname, influx_username, influx_password);
            
        }
            

        static void Main(string[] args)
        {
            try
            {


                Task t = ExecuteHourlyRevenue();

                t.Wait();
                logger.Info("END");
            }
            catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        private static long ToUnixTimeSeconds(DateTime dt)
        {
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
#if DEBUG
            long unixTime = (dt.AddHours(-9).Ticks - epochTicks) / TimeSpan.TicksPerSecond;
#else
            long unixTime = (dt.Ticks - epochTicks) / TimeSpan.TicksPerSecond;
#endif
            return unixTime;
        }

        private static async Task ExecuteHourlyRevenue()
        {
            List<IInfluxDatapoint> influxDatapoints = new List<IInfluxDatapoint>();
            using (var session = dataAccessor.SessionFactory.OpenSession())
            using (var trans = session.BeginTransaction())
            {
                PeiuPlatformMethodsExecutor methodsExecutor = new PeiuPlatformMethodsExecutor(session);
                var results = await session.CreateCriteria<HourlyAccmofMeasurement>().ListAsync<HourlyAccmofMeasurement>();
                foreach (HourlyAccmofMeasurement row in results)
                {
                    HourlyActualRevenue hourlyRevenue = new HourlyActualRevenue();
                    hourlyRevenue.Createdt = row.Createdt;
                    hourlyRevenue.Hour = row.Hour;
                    hourlyRevenue.Rcc = row.Rcc;
                    hourlyRevenue.Siteid = row.Siteid;

                    double pvPower = row.Sumofpvgeneration;
                    double dhg = row.Sumofdischarge;

                    int? rec = methodsExecutor.GetRec(row.Createdt);
                    float? smp = methodsExecutor.GetSmpPrice(row.Rcc == 16  ?  1 : 0, row.Createdt);

                    //hourlyRevenue.Rec = rec.HasValue ? rec.Value : 0;
                    //hourlyRevenue.Smp = smp.HasValue ? smp.Value : 0;
                    if (rec.HasValue && smp.HasValue)
                    {
                        double sumOfChg = row.Sumofcharge * -1;
                        double exp1 = ((pvPower - sumOfChg) * 1 * rec.Value / 1000) + (dhg * 5 * rec.Value / 1000);
                        double exp2 = (((pvPower - sumOfChg) * 1) + (dhg * 5)) * (rec.Value / 1000);
                        hourlyRevenue.Revenue = exp1 + exp2;
                    }
                    else
                        hourlyRevenue.Revenue = 0;


                    await session.SaveOrUpdateAsync(hourlyRevenue);
                    IInfluxDatapoint datapoint = ConvertinfluxDatapoint(hourlyRevenue);
                    influxDatapoints.Add(datapoint);
                }
                await trans.CommitAsync();
            }
            logger.Info("Complete record to mysql");
            await influxDBClient.PostPointsAsync("statistics", influxDatapoints, influxDatapoints.Count);
            logger.Info("Completed record to influxdb");
        }

        private static async Task Run(DateTime? dt)
        {
            DateTime date = DateTime.Now.Date.AddHours(DateTime.Now.Hour - 1);
            if (dt.HasValue)
                date = dt.Value;
            logger.Info("RUN Datetime: " + date);
            long tps = ToUnixTimeSeconds(date);
            long endTps = ToUnixTimeSeconds(date.AddHours(1));
            logger.Info("Reading A4 data from influxdb...");
            List<IInfluxSeries> result = await Select(tps, endTps, "sum_chg_1h", "sum_dhg_1h", "sum_pv_1h");
            IInfluxSeries pvSeries = result.FirstOrDefault(x => x.SeriesName == "sum_pv_1h");
            IInfluxSeries chgSeries = result.FirstOrDefault(x => x.SeriesName == "sum_chg_1h");
            IInfluxSeries dhgSeries = result.FirstOrDefault(x => x.SeriesName == "sum_dhg_1h");

            List<IInfluxDatapoint> influxDatapoints = new List<IInfluxDatapoint>();
            using (var session = dataAccessor.SessionFactory.OpenSession())
            using(var trans  = session.BeginTransaction())
            {
                PeiuPlatformMethodsExecutor methodsExecutor = new PeiuPlatformMethodsExecutor(session);
                foreach (dynamic row in pvSeries.Entries)
                {
                    string siteId = row.SiteId;
                    string rccString = row.Rcc;
                    DateTime time = row.Time;

                    TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
                    DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(time, timeZoneInfo);
                    string inJeju = row.InJeju;
                    string sumOfPvPower = row.SumOfPvPower;

                    dynamic chg_row = chgSeries.Entries.FirstOrDefault(x => x.Time == time && x.SiteId == siteId);
                    dynamic dhg_row = dhgSeries.Entries.FirstOrDefault(x => x.Time == time && x.SiteId == siteId);
                    PeiuPlatform.Models.Mysql.HourlyAccmofMeasurement newLine = new Models.Mysql.HourlyAccmofMeasurement();
                    HourlyActualRevenue hourlyRevenue = new HourlyActualRevenue();
                    newLine.Createdt = hourlyRevenue.Createdt = localTime.Date;
                    newLine.Hour = hourlyRevenue.Hour = localTime.Hour;
                    newLine.Inisland = hourlyRevenue.Inisland = inJeju.Equals("1") ? true : false;
                    int rcc = int.Parse(rccString);
                    newLine.Rcc = hourlyRevenue.Rcc = rcc;
                    newLine.Siteid = hourlyRevenue.Siteid = int.Parse(siteId);

                    double pvPower = double.Parse(sumOfPvPower);
                    double dhg = dhg_row == null ? 0d : double.Parse(dhg_row.SumOfDhg);

                    newLine.Sumofpvgeneration = pvPower;
                    newLine.Sumofcharge = chg_row == null ? 0d : double.Parse(chg_row.SumOfChg);
                    newLine.Sumofdischarge = dhg;

                    int? rec = methodsExecutor.GetRec(localTime.Date);
                    float? smp = methodsExecutor.GetSmpPrice(int.Parse(inJeju), localTime.Date);
                    //hourlyRevenue.Rec = rec.HasValue ? rec.Value : 0;
                    //hourlyRevenue.Smp = smp.HasValue ? smp.Value : 0;
                    if (rec.HasValue && smp.HasValue)
                    {
                        double sumOfChg = newLine.Sumofcharge * -1;
                        double exp1 = ((pvPower - sumOfChg) * 1 * rec.Value / 1000) + (dhg * 5 * rec.Value / 1000);
                        double exp2 = (((pvPower - sumOfChg) * 1) + (dhg * 5)) * (rec.Value / 1000);
                        hourlyRevenue.Revenue = exp1 + exp2;
                    }
                    else
                        hourlyRevenue.Revenue = 0;

                    IInfluxDatapoint datapoint = ConvertinfluxDatapoint(hourlyRevenue);
                    influxDatapoints.Add(datapoint);
                    await session.SaveOrUpdateAsync(newLine);
                    await session.SaveOrUpdateAsync(hourlyRevenue);
                }
                await trans.CommitAsync();
            }
            logger.Info("Completed record to mysql");
            await influxDBClient.PostPointsAsync("statistics", influxDatapoints, influxDatapoints.Count);
            logger.Info("Completed record to influxdb");
            logger.Info("Complete");
        }

        private static IInfluxDatapoint ConvertinfluxDatapoint(HourlyActualRevenue revenue)
        {
            var valMixed = new InfluxDatapoint<InfluxValueField>();
            DateTime dt = revenue.Createdt.AddHours(revenue.Hour);

            valMixed.UtcTimestamp = dt.AddHours(-9);
            valMixed.Tags.Add("siteId", revenue.Siteid.ToString());
            valMixed.Tags.Add("rcc", revenue.Rcc.ToString());
            valMixed.Fields.Add("revenue", new InfluxValueField(revenue.Revenue));
            valMixed.MeasurementName = "revenue_1h";
            valMixed.Precision = TimePrecision.Hours;
            return valMixed;
        }

        private static async Task<List<IInfluxSeries>> Select(long start, long end, params string[] measurements)
        {
            try

            {
                List<IInfluxSeries> queries = new List<IInfluxSeries>();
                foreach (string measurement in measurements)
                {
                    string query = $"select * from {measurement} where time >= {start}s and time < {end}s";
                    logger.Info("INFLUX QUERY: " + query);
                    var result = await influxDBClient.QueryMultiSeriesAsync("statistics", query);
                    logger.Info("INFLUX QUERY RESULT COUNT: " + result.Count);
                    if (result.Count > 0)
                        queries.Add(result[0]);
                }
                //string query = $"select * from {measurements} where time = {start}s";
                //if (add_where_clause != null)
                //    query = query + " and " + add_where_clause;
                return queries;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        private static async Task<List<IInfluxSeries>> Select(long start, bool BetterThan, params string[] measurements)
        {
            try

            {
                string exp = BetterThan ? ">=" : "=";
                List<IInfluxSeries> queries = new List<IInfluxSeries>();
                foreach (string measurement in measurements)
                {
                    string query = $"select * from {measurement} where time {exp} {start}s";
                    logger.Info("INFLUX QUERY: " + query);
                    var result = await influxDBClient.QueryMultiSeriesAsync("statistics", query);
                    if (result.Count > 0)
                        queries.Add(result[0]);
                }
                //string query = $"select * from {measurements} where time = {start}s";
                //if (add_where_clause != null)
                //    query = query + " and " + add_where_clause;
                return queries;
            }
            catch(Exception ex)
                {
                Console.WriteLine(ex.Message);
                throw ex;
                }
        }
    }
}
