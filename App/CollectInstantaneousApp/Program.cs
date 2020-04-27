using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PeiuPlatform.DataAccessor;
using PeiuPlatform.Models.Mysql;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace CollectInstantaneousApp
{
    class Program
    {
        static NLog.Logger logger;
        static RedisDataAccessor RedisDataAccessor;
        static IDatabase redisDatabase;
        static IInfluxDataAccess influxDataAccess;
        static TimeZoneInfo timeZoneInfo;
        static void Main(string[] args)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            System.Reflection.Assembly executeAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            DateTime lastBuildDate =
                new System.IO.FileInfo(executeAssembly.Location).LastWriteTime;
            logger.Info($"CollectInstantaneous App Start. Last build:{lastBuildDate.ToShortDateString()} (utc)");

            try
            {
#if DEBUG
                string fileName = "redisconfig.Development.json";
#else
                string fileName = "redisconfig.json";
                timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
#endif

                string redis_config_full_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                RedisConfiguration config = JsonConvert.DeserializeObject<RedisConfiguration>(File.ReadAllText(redis_config_full_path));
                influxDataAccess = InfluxDataAccess.CreateDataAccessFromEnvironment();
                RedisDataAccessor = new RedisDataAccessor(config);
                redisDatabase = RedisDataAccessor.GetDatabase();
                Task worker = Run();
                worker.Wait();
            }
            catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
            }

        }

        static async Task Run()
        {
            MysqlDataAccessor mysqlData = MysqlDataAccessor.CreateDataAccessFromEnvironment();
            using (var session = mysqlData.SessionFactory.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var sites = await session.CreateCriteria<Vwcontractorsite>().ListAsync<Vwcontractorsite>();
                logger.Info("Start calculate Minute Measurement");
                Dictionary<int, List<string>> pcsKeys = null;
                Dictionary<int, List<string>> bmsKeys = null;
                Dictionary<int, List<string>> pvKeys = null;


                DateTime now = DateTime.Now.ToUniversalTime();
#if DEBUG
                DateTime stampDate = now;
#else
                DateTime stampDate = now.AddHours(9);
#endif
                DateTime accum_date = stampDate.AddMinutes(-1);

                DateTime startDt = now.Date.AddHours(now.Hour).AddMinutes(now.Minute - 1);
                DateTime endDt = startDt.AddMinutes(1).AddSeconds(-1);
                CreateRedisKeys(sites, ref pcsKeys, ref bmsKeys, ref pvKeys);
                foreach(var x in sites)
                {
                    double soc_men = await MeanAsync(bmsKeys[x.Siteid], "bms_soc");
                    double soh_men = await MeanAsync(bmsKeys[x.Siteid], "bms_soh");
                    double act_sum = await SumAsync(pcsKeys[x.Siteid], "actPwrKw");
                    double pv_sum = await SumAsync(pvKeys[x.Siteid], "TotalActivePower");

                    double sumOfCharge = await influxDataAccess.Sum("peiudb", "pcs_data", "actPwrKw", x.Siteid, startDt, endDt, "actPwrKw < 0");
                    double sumOfDischarge = await influxDataAccess.Sum("peiudb", "pcs_data", "actPwrKw", x.Siteid, startDt, endDt, "actPwrKw > 0");
                    double avgOfSoc = await influxDataAccess.Average("peiudb", "bms_data", "bms_soc", x.Siteid, startDt, endDt);
                    double avgOfSoh = await influxDataAccess.Average("peiudb", "bms_data", "bms_soh", x.Siteid, startDt, endDt);
                    double sumofpvgeneration = await influxDataAccess.Sum("peiudb", "pv_data", "TotalActivePower", x.Siteid, startDt, endDt);
                    MinuteMeasurement instantaneous = new MinuteMeasurement();
                    instantaneous.Rcc = x.Rcc;
                    instantaneous.Siteid = x.Siteid;
                    instantaneous.Createdt = stampDate.Date;
                    instantaneous.Hour = stampDate.Hour;
                    instantaneous.Minute = stampDate.Minute;
                    instantaneous.Inisland = x.Inisland;
                    instantaneous.Soc = soc_men;
                    instantaneous.Soh = soh_men;
                    instantaneous.Activepower = act_sum;
                    instantaneous.Pvgeneration = pv_sum;
                    await session.SaveOrUpdateAsync(instantaneous);

                    MinuteAccmofMeasurement accum = new MinuteAccmofMeasurement();
                    accum.Rcc = x.Rcc;
                    accum.Siteid = x.Siteid;
                    accum.Createdt = accum_date.Date;
                    accum.Hour = accum_date.Hour;
                    accum.Minute = accum_date.Minute;
                    accum.Avgofsoc = avgOfSoc;
                    accum.Avgofsoh = avgOfSoh;
                    accum.Sumofcharge = sumOfCharge / 3600;
                    accum.Sumofdischarge = sumOfDischarge / 3600;
                    accum.Sumofpvgeneration = sumofpvgeneration / 3600;
                    await session.SaveOrUpdateAsync(accum);
                }
                logger.Info("End calculate Minute Measurement");
                logger.Info("Start calculate Minute Accumulate");
                



                await transaction.CommitAsync();
                
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


        private static async Task<double> MeanAsync(List<string> keys, string hashFieldName)
        {
            List<double> listValue = new List<double>();
            foreach(string key in keys)
            {
                double value = (double)await redisDatabase.HashGetAsync(key, hashFieldName);
                listValue.Add(value);
            }
            return listValue.Average();
        }

        private static async Task<double> SumAsync(List<string> keys, string hashFieldName)
        {
            List<double> listValue = new List<double>();
            foreach (string key in keys)
            {
                double value = (double)await redisDatabase.HashGetAsync(key, hashFieldName);
                listValue.Add(value);
            }
            return listValue.Sum();
        }

        private static void CreateRedisKeys(IEnumerable<Vwcontractorsite> sites, ref Dictionary<int, List<string>> pcsKeys, ref Dictionary<int, List<string>> bmsKeys, ref Dictionary<int, List<string>> pvKeys)
        {
            pcsKeys = new Dictionary<int, List<string>>();
            bmsKeys = new Dictionary<int, List<string>>();
            pvKeys = new Dictionary<int, List<string>>();
            foreach (Vwcontractorsite site in sites)
            {
                if (pcsKeys.ContainsKey(site.Siteid) == false)
                    pcsKeys.Add(site.Siteid, new List<string>());
                if (bmsKeys.ContainsKey(site.Siteid) == false)
                    bmsKeys.Add(site.Siteid, new List<string>());
                if (pvKeys.ContainsKey(site.Siteid) == false)
                    pvKeys.Add(site.Siteid, new List<string>());
                pcsKeys[site.Siteid].AddRange(createdeviceKeys(site.Siteid, 1, "PCS", site.Pcscount));
                bmsKeys[site.Siteid].AddRange(createdeviceKeys(site.Siteid, 2, "BMS", site.Bmscount));
                pvKeys[site.Siteid].AddRange(createdeviceKeys(site.Siteid, 4, "PV", site.Pvcount));
            }
        }

        private static IEnumerable<string> createdeviceKeys(int siteid, int groupid, string devicename, decimal? deviceCnt)
        {
            for(int i=1;i<=deviceCnt;i++)
            {
                yield return $"SID{siteid}.GID{groupid}.{devicename}{i}";
            }
        }
    }
}
