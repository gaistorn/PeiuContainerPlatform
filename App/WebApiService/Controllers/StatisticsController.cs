using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using PEIU.Events.Alarm;
using PEIU.Models;
using PEIU.Models.Database;
using PEIU.Models.DatabaseModel;
using PEIU.Models.IdentityModel;
using PEIU.Service;
using PeiuPlatform.App;
using PeiuPlatform.App.Localization;
using PES.Toolkit;
using StackExchange.Redis;

namespace WebApiService.Controllers
{
    [Route("api/statistics")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly UserManager<UserAccountEF> _userManager;
        AccountEF _accountContext;
        private readonly SignInManager<UserAccountEF> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<Role> roleManager;
        private readonly IHTMLGenerator htmlGenerator;
        private readonly GridDataContext _peiuGridDataContext;
        private readonly IDatabaseAsync _redisDb;
        private readonly ConnectionMultiplexer _redisConn;
        private readonly KpxDataContext kpxDataContext;
        IClaimServiceFactory claimsManager;
        readonly ILogger<StatisticsController> logger;

        public StatisticsController(UserManager<UserAccountEF> userManager,
            SignInManager<UserAccountEF> signInManager, RoleManager<Role> _roleManager, IRedisConnectionFactory redis, ILogger<StatisticsController> logger,
            IEmailSender emailSender, IHTMLGenerator _htmlGenerator,  GridDataContext peiuGridDataContext, KpxDataContext kpxDataContext, IClaimServiceFactory claimsManager,
            AccountEF accountContext)
        {
            _userManager = userManager;
            _accountContext = accountContext;
            _signInManager = signInManager;
            _emailSender = emailSender;
            htmlGenerator = _htmlGenerator;
            roleManager = _roleManager;
            _peiuGridDataContext = peiuGridDataContext;
            _redisConn = redis.Connection();
            this.kpxDataContext = kpxDataContext;
            _redisDb = _redisConn.GetDatabase();
            this.claimsManager = claimsManager;
            this.logger = logger;
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getmonthlystatisticsbysiteid")]
        public async Task<IActionResult> GetMonthlyStatisticsBySiteId(int year, int month, int siteId)
        {
            try
            {
                DateTime startDt = new DateTime(year, month, 1);
                DateTime endDt = startDt.AddMonths(1).AddDays(-1);
                IEnumerable<int> siteIds = null;
                siteIds = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
                if (siteIds.Contains(siteId) == false)
                    return Unauthorized();

                JObject result = new JObject();

                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    IList<DailyStatistic> result_pcs = await session.CreateCriteria<DailyStatistic>()
                        .Add(Restrictions.Ge("Timestamp", startDt) && Restrictions.Le("Timestamp", endDt) && Restrictions.Eq("SiteId", siteId))
                        .ListAsync<DailyStatistic>();

                    result.Add("sumOfCharging", result_pcs.Sum(x => x.Charging));
                    result.Add("sumOfDischarging", result_pcs.Sum(x => x.Discharging));
                    result.Add("avgOfOperationRate", result_pcs.Average(x => x.Operationrate));


                    IList<DailyStatisticsPv> result_pv = await session.CreateCriteria<DailyStatisticsPv>()
                        .Add(Restrictions.Ge("Timestamp", startDt) && Restrictions.Le("Timestamp", endDt) && Restrictions.Eq("SiteId", siteId))
                        .ListAsync<DailyStatisticsPv>();

                    result.Add("sumOfPvPower", result_pv.Sum(x => x.Accumpvpower));
                    return Ok(result);

                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getdailystatisticsbysiteid")]
        public async Task<IActionResult> GetDailyStatisticsBySiteId(DateTime date, int siteId)
        {
            try
            {
                if (date == default(DateTime))
                    return BadRequest();
                IEnumerable<int> siteIds = null;
                siteIds = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
                if (siteIds.Contains(siteId) == false)
                    return Unauthorized();

                JObject result = new JObject();

                using(var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    IList<DailyStatistic> result_pcs = await session.CreateCriteria<DailyStatistic>()
                        .Add(Restrictions.Eq("Timestamp", date.Date) && Restrictions.Eq("SiteId", siteId))
                        .ListAsync<DailyStatistic>();

                    result.Add("sumOfCharging", result_pcs.Sum(x => x.Charging));
                    result.Add("sumOfDischarging", result_pcs.Sum(x => x.Discharging));
                    result.Add("avgOfOperationRate", result_pcs.Average(x => x.Operationrate));


                    IList<DailyStatisticsPv> result_pv = await session.CreateCriteria<DailyStatisticsPv>()
                        .Add(Restrictions.Eq("Timestamp", date.Date) && Restrictions.Eq("SiteId", siteId))
                        .ListAsync<DailyStatisticsPv>();

                    result.Add("sumOfPvPower", result_pv.Sum(x => x.Accumpvpower));
                    return Ok(result);
                    
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
        }

        
        /// <summary>
        /// 분단위 통계 이력 (사이트 ID별)
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getminutestatisticsbysiteid")]
        public async Task<IActionResult> GetMinuteStatisticsBySiteId(DateTime startDate, DateTime endDate, int siteId = -1)
        {
            try
            {
                IEnumerable<int> siteIds = null;
                if (siteId == -1)
                    siteIds = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
                else
                    siteIds = new int[] { siteId };

                var result = await GetMinuteStat(startDate, endDate, siteIds);
                if (result == null)
                    return BadRequest();
                else
                    return Ok(result);

            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
        }

        private IEnumerable<int> GetSiteIdsByRcc(int rcc)
        {
            IEnumerable<int>  siteIds = ControlHelper.GetAvaliableSites(_accountContext, claimsManager, HttpContext).Where(x => x.RCC == rcc).Select(x => x.SiteId).Distinct();
            return siteIds;
        }


        static ConcurrentDictionary<string, IEnumerable<RedisKey>> siteIdKeys = new ConcurrentDictionary<string, IEnumerable<RedisKey>>();
        static DateTime lastUpdateSiteIdFromRedis = DateTime.MinValue;
        /// <summary>
        /// RCC별 충/방전, PV 발전, SOC, PCS-PV-BMS 설치용량 요청 (실시간 데이터)
        /// </summary>
        /// <returns>충/방전, PV 발전, SOC, PCS-PV-BMS 설치용량</returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getstatisticscurrentvaluebyrcc")]
        public async Task<IActionResult> GetStatisticsCurrentValueByRcc()
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            JObject row_j = null;
            try
            {
                IEnumerable<IGrouping<int, VwContractorsiteEF>> idsGroup = ControlHelper.GetAvaliableRccEntities(_accountContext, claimsManager, HttpContext);
                
                JArray result = new JArray();
                if (idsGroup.Count() == 0)
                    return Ok(result);
                List<double> total_socs = new List<double>();
                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    foreach (IGrouping<int, VwContractorsiteEF> row in idsGroup)
                    {
                        double total_energy_power = 0;
                        double total_actPwr_charging = 0;
                        double total_actPwr_discharging = 0;

                        double capacity_pcs = 0;
                        double capacity_bms = 0;
                        double capacity_pv = 0;
                        int event_count = 0;
                        List<double> socs = new List<double>();
                        List<double> sohs = new List<double>();
                        JObject weather_obj = null;

                        bool IsUpdate = lastUpdateSiteIdFromRedis < DateTime.Now;

                        foreach (VwContractorsiteEF site in row)
                        {
                            int SiteId = site.SiteId;
                            capacity_pcs += site.TotalPcsCapacity ?? 0;
                            capacity_bms += site.TotalBmsCapacity ?? 0;
                            capacity_pv += site.TotalPvCapacity ?? 0;
                            if (weather_obj == null)
                                weather_obj = await CommonFactory.RetriveWeather(SiteId, _redisDb);

                            var query_results = await session.CreateCriteria<VwEventRecord>()
                       .Add(Restrictions.IsNull("RecoveryDT") && Restrictions.InG<int>("SiteId", row.Select(x => x.SiteId)))
                       .ListAsync<VwEventRecord>();
                            event_count = +query_results.Count;

                            // PV 
                            string target_redis_key = CommonFactory.CreateRedisKey(SiteId, 4, "PV*");
                            if(IsUpdate || siteIdKeys.ContainsKey(target_redis_key) == false)
                            {
                                siteIdKeys[target_redis_key] = CommonFactory.SearchKeys(_redisConn, target_redis_key);
                            }
                            var redis_keys = siteIdKeys[target_redis_key];
                            foreach (RedisKey pv_key in redis_keys)
                            {
                                double TotalActivePower = (double)await _redisDb.HashGetAsync(pv_key, "TotalActivePower");
                                total_energy_power += TotalActivePower;
                            }

                            // PCS
                            target_redis_key = CommonFactory.CreateRedisKey(SiteId, 1, "PCS*");

                            if (IsUpdate || siteIdKeys.ContainsKey(target_redis_key) == false)
                            {
                                siteIdKeys[target_redis_key] = CommonFactory.SearchKeys(_redisConn, target_redis_key);
                            }
                            redis_keys = siteIdKeys[target_redis_key];
                            foreach (RedisKey key in redis_keys)
                            {
                                double TotalActivePower = (double)await _redisDb.HashGetAsync(key, "actPwrKw");
                                if (TotalActivePower > 0)
                                    total_actPwr_discharging += TotalActivePower;
                                else
                                    total_actPwr_charging += TotalActivePower;

                            }

                            // BMS
                            target_redis_key = CommonFactory.CreateRedisKey(SiteId, 2, "BMS*");
                            if (IsUpdate || siteIdKeys.ContainsKey(target_redis_key) == false)
                            {
                                siteIdKeys[target_redis_key] = CommonFactory.SearchKeys(_redisConn, target_redis_key);
                            }
                            redis_keys = siteIdKeys[target_redis_key];
                            foreach (RedisKey key in redis_keys)
                            {
                                double soc = (double)await _redisDb.HashGetAsync(key, "bms_soc");
                                socs.Add(soc);
                                total_socs.Add(soc);
                                double soh = (double)await _redisDb.HashGetAsync(key, "bms_soh");
                                sohs.Add(soh);
                            }
                        }

                        if (IsUpdate)
                            lastUpdateSiteIdFromRedis = DateTime.Now.AddMinutes(10);

                        row_j = new JObject();
                        row_j.Add("rcc", row.Key);
                        row_j.Add("total_pvpower", total_energy_power);
                        row_j.Add("total_charging", Math.Abs(total_actPwr_charging));
                        row_j.Add("total_discharging", total_actPwr_discharging);
                        row_j.Add("total_pcs_capacity", capacity_pcs);
                        row_j.Add("total_bms_capacity", capacity_bms);
                        row_j.Add("total_pv_capacity", capacity_pv);
                        row_j.Add("average_soc", socs.Count() > 0 ? socs.Average() : 0);
                        row_j.Add("average_soh", sohs.Count() > 0 ? sohs.Average() : 0);
                        row_j.Add("total_site_count", row.Count());
                        row_j.Add("total_event_count", event_count);
                        row_j.Add("weather", weather_obj);
                        result.Add(row_j);
                    }

                    JObject center_weather = await CommonFactory.RetriveWeather(0, _redisDb);

                    double total_socs_avg = total_socs.Count > 0 ? total_socs.Average() : 0;
                    return Ok(new { group = result, total_average_soc = total_socs_avg, total_event_count = 0, controlcenter_weather = center_weather });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// 분단위 통계 이력 (aggId 별)
        /// </summary>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <param name="aggid"></param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.RequiredManager)]
        [HttpGet, Route("getminutestatisticsbyaggid")]
        public async Task<IActionResult> GetMinuteStatisticsByAggId(DateTime startdate, DateTime enddate, string aggid)
        {
            var siteids = ControlHelper.GetAvaliableSiteIdsByAggGroupId(_accountContext, claimsManager, HttpContext, aggid);
            var result = await GetMinuteStat(startdate, enddate, siteids);
            if (result == null)
                return BadRequest();
            else
                return Ok(result);
        }

        /// <summary>
        /// 분단위 통계 이력 (rcc별)
        /// </summary>
        /// <param name="startdate">시작날짜</param>
        /// <param name="enddate">종료날짜</param>
        /// <param name="rcc">rcc번호</param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getminutestatisticsbyrcc")]
        public async Task<IActionResult> GetMinuteStatisticsByRcc(DateTime startdate, DateTime enddate, int rcc)
        {
            IEnumerable<int> siteIds = GetSiteIdsByRcc(rcc);
            var result = await GetMinuteStat(startdate, enddate, siteIds);
            if (result == null)
                return BadRequest();
            else
                return Ok(result);
        }
        
        /// <summary>
        ///  RCC별 시간당 누적데이터 이력 (충/방전, PV발전) / rcc별
        /// </summary>
        /// <param name="startdate">시작날짜</param>
        /// <param name="enddate">종료날짜</param>
        /// <param name="rcc">rcc 번호</param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("gethourlystatisticsbyrcc")]
        public async Task<IActionResult> GetHourlyStatisticsByRcc(DateTime startdate, DateTime enddate, int rcc)
        {
            try
            {
                IEnumerable<int> siteIds = GetSiteIdsByRcc(rcc);
                var result = await GetHourlyStat(startdate, enddate, siteIds);
                if (result == null)
                    return BadRequest();
                else
                    return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
        }

        /// <summary>
        /// RCC별 시간당 누적데이터 이력 (충/방전, PV발전) / AggId 별
        /// </summary>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <param name="aggid"></param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("gethourlystatisticsbyaggid")]
        public async Task<IActionResult> GetHourlyStatisticsByAggId(DateTime startdate, DateTime enddate, string aggid)
        {
            try
            {
                var siteids = ControlHelper.GetAvaliableSiteIdsByAggGroupId(_accountContext, claimsManager, HttpContext, aggid);
                var result = await GetHourlyStat(startdate, enddate, siteids);
                if (result == null)
                    return BadRequest();
                else
                    return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
        }

        /// <summary>
        /// RCC별 전체 이벤트 갯수
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getsummaryeventcountbyrcc")]
        public async Task<IActionResult> GetSummaryEventCountByRcc()
        {
            JObject return_Result = new JObject();
            IEnumerable<IGrouping<int, int>> rcc_group = ControlHelper.GetAvaliableRccCodes(_accountContext, claimsManager, HttpContext);
            using (var session = _peiuGridDataContext.SessionFactory.OpenStatelessSession())
            {
                JArray jObject = new JArray();
                int totalCnt = 0;
                foreach (IGrouping<int, int> rcc_row in rcc_group)
                { 
                    JObject jRow = new JObject();
                    var query_results = await session.CreateCriteria<VwEventRecord>()
                        .Add(Restrictions.IsNull("AckDT") && Restrictions.InG<int>("SiteId", rcc_row))
                        .ListAsync<VwEventRecord>();
                    int cnt = query_results.Count;
                    jRow.Add($"rcc_code", rcc_row.Key);
                    jRow.Add("count", cnt);
                    jObject.Add(jRow);
                    totalCnt += cnt;
                }

                return_Result.Add("result", jObject);
                return_Result.Add("total", totalCnt);
            }
            return Ok(return_Result);


        }

        /// <summary>
        ///  SiteId별 시간당 누적데이터 이력 (충/방전, PV발전)
        /// </summary>
        /// <param name="startdate">시작날짜</param>
        /// <param name="enddate">종료날짜</param>
        /// <param name="siteId">사이트 아이디 (없을 경우 전체)</param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("gethourlystatisticsbysiteid")]
        public async Task<IActionResult> GetHourlyStatisticsBySiteId(DateTime startdate, DateTime enddate, int siteId = -1)
        {
            try
            {
                IEnumerable<int> siteIds = null;
                if (siteId == -1)
                    siteIds = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
                else
                    siteIds = new int[] { siteId };

                var result = await GetHourlyStat(startdate, enddate, siteIds);
                if (result == null)
                    return BadRequest();
                else
                    return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
        }

        private async Task<JObject> GetMinuteStat(DateTime startDate, DateTime endDate, IEnumerable<int> siteIds)
        {
            try
            {
                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    JObject result = new JObject();

                    var pcs_result = await MethodsExecutor.GetMinuteStatBySiteId(session, startDate, endDate, siteIds);

                    var pv_result = await MethodsExecutor.GetMinutepvStatBySiteId(session, startDate, endDate, siteIds);

                    var bms_result = await MethodsExecutor.GetMinuteBmsStatBySiteid(session, startDate, endDate, siteIds);
                    //var join_query = pcs_result.Join<(DateTime x, int y, double z), (pv_result, 
                    //    pcs => new { pcs.SiteId, pcs.Timestamp },
                    //    pv => new { pv.

                    var result_query = from pcs in pcs_result
                                       join pv in pv_result
                                       on new { condition1 = pcs.Timestamp, condition2 = pcs.SiteId } equals new { condition1 = pv.Timestamp, condition2 = pv.SiteId }
                                       join bms in bms_result
                                       on new { condition1 = pcs.Timestamp, condition2 = pcs.SiteId } equals new { condition1 = bms.Timestamp, condition2 = bms.SiteId }
                                       select new
                                       {
                                           Timestamp = pcs.Timestamp,
                                           siteid = pcs.SiteId,
                                           SumOfActPwr = pcs.Chg + pcs.Dhg,
                                           SumOfPvPower = pv.Accumpvpower,
                                           AvgOfSoc = bms.Soc,
                                           AvgOfSoh = bms.Soh
                                       };

                    var g_result = result_query.GroupBy(key => key.Timestamp, value => value);


                    JArray actPwrs = new JArray();
                    JArray pwPwrs = new JArray();
                    JArray timestamps = new JArray();
                    JArray bmsArr = new JArray();
                    foreach (var row in g_result)
                    {
                        timestamps.Add(row.Key);
                        actPwrs.Add(row.Sum(x => x.SumOfActPwr));
                        pwPwrs.Add(row.Sum(x => x.SumOfPvPower));
                        bmsArr.Add(row.Average(x => x.AvgOfSoc));
                    }

                    result.Add("timestamps", timestamps);
                    result.Add("sumOfActPwrs", actPwrs);
                    result.Add("sumOfPvPwrs", pwPwrs);
                    result.Add("avgOfSoc", bmsArr);
                    return  result;

                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return null;
            }
        }

        private async Task<JObject> GetHourlyStat(DateTime startdate, DateTime enddate, int rcc)
        {
            try
            {
                using (var statelessSession = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    JObject result = new JObject();

                    var pcs_result = await MethodsExecutor.GetMinuteStatByRcc(statelessSession, startdate.Date, enddate.Date, rcc);

                    var pv_result = await MethodsExecutor.GetMinutepvStatByRcc(statelessSession, startdate.Date, enddate.Date, rcc);

                    var bms_result = await MethodsExecutor.GetMinuteBmsStatByRcc(statelessSession, startdate.Date, enddate.Date, rcc);

                    //List<(DateTime timestamp, float actPwr)> result_list = new List<(DateTime timestamp, float actPwr)>();



                    //var join_query = pcs_result.Join<(DateTime x, int y, double z), (pv_result, 
                    //    pcs => new { pcs.SiteId, pcs.Timestamp },
                    //    pv => new { pv.

                    var result_query = from pcs in pcs_result
                                       join pv in pv_result
                                       on new { condition1 = pcs.Timestamp, condition2 = pcs.SiteId } equals new { condition1 = pv.Timestamp, condition2 = pv.SiteId }
                                       join bms in bms_result
                                       on new { condition1 = pcs.Timestamp, condition2 = pcs.SiteId } equals new { condition1 = bms.Timestamp, condition2 = bms.SiteId }
                                       select new
                                       {
                                           Timestamp = pcs.Timestamp,
                                           siteid = pcs.SiteId,
                                           SumOfActPwr = pcs.Chg + pcs.Dhg,
                                           SumOfPvPower = pv.Accumpvpower,
                                           AvgOfSoc = bms.Soc,
                                           AvgOfSoh = bms.Soh
                                       };

                    var g_result = result_query.GroupBy(key => key.Timestamp, value => value);


                    JArray actPwrs = new JArray();
                    JArray pwPwrs = new JArray();
                    JArray timestamps = new JArray();
                    JArray socArray = new JArray();
                    foreach (var row in g_result)
                    {

                        timestamps.Add(row.Key);
                        actPwrs.Add(row.Sum(x => x.SumOfActPwr / 60));
                        pwPwrs.Add(row.Sum(x => x.SumOfPvPower / 60));
                        socArray.Add(row.Average(x => x.AvgOfSoc));
                    }
                    result.Add("timestamps", timestamps);
                    result.Add("sumOfActPwrs", actPwrs);
                    result.Add("sumOfPvPwrs", pwPwrs);
                    result.Add("avgOfSoc", socArray);

                    return result;

                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return null;
            }
        }

        private async Task<JObject> GetHourlyStat(DateTime startdate, DateTime enddate, IEnumerable<int> siteIds)
        {
            try
            {
                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    JObject result = new JObject();

                    var pcs_result = await MethodsExecutor.GetMinuteStatBySiteId(session, startdate, enddate, siteIds);

                    var pv_result = await MethodsExecutor.GetMinutepvStatBySiteId(session, startdate, enddate, siteIds);

                    var bms_result = await MethodsExecutor.GetMinuteBmsStatBySiteid(session, startdate, enddate, siteIds);

                    //List<(DateTime timestamp, float actPwr)> result_list = new List<(DateTime timestamp, float actPwr)>();

                  

                    //var join_query = pcs_result.Join<(DateTime x, int y, double z), (pv_result, 
                    //    pcs => new { pcs.SiteId, pcs.Timestamp },
                    //    pv => new { pv.

                    var result_query = from pcs in pcs_result
                                       join pv in pv_result
                                       on new { condition1 = pcs.Timestamp, condition2 = pcs.SiteId } equals new { condition1 = pv.Timestamp, condition2 = pv.SiteId }
                                       join bms in bms_result
                                       on new { condition1 = pcs.Timestamp, condition2 = pcs.SiteId } equals new { condition1 = bms.Timestamp, condition2 = bms.SiteId }
                                       select new
                                       {
                                           Timestamp = pcs.Timestamp,
                                           siteid = pcs.SiteId,
                                           SumOfActPwr = pcs.Chg + pcs.Dhg,
                                           SumOfPvPower = pv.Accumpvpower,
                                           AvgOfSoc = bms.Soc,
                                           AvgOfSoh = bms.Soh
                                       };

                    var g_result = result_query.GroupBy(key => key.Timestamp.ToString("yyyyMMddHH"), value => value);


                    JArray actPwrs = new JArray();
                    JArray pwPwrs = new JArray();
                    JArray timestamps = new JArray();
                    JArray socArray = new JArray();
                    foreach (var row in g_result)
                    {
                        DateTime timestamp = DateTime.ParseExact(row.Key, "yyyyMMddHH", null);
                        timestamps.Add(timestamp);
                        actPwrs.Add(row.Sum(x => x.SumOfActPwr / 60));
                        pwPwrs.Add(row.Sum(x => x.SumOfPvPower / 60));
                        socArray.Add(row.Average(x => x.AvgOfSoc));
                    }
                    result.Add("timestamps", timestamps);

                    result.Add("sumOfActPwrs", actPwrs);
                    result.Add("sumOfPvPwrs", pwPwrs);
                    result.Add("avgOfSoc", socArray);

                    return result;

                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 금월 충/방전, PV발전 누적 데이터
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getmonthlyaccumuactivepower")]
        public async Task<IActionResult> GetMonthlyAccumuActivePower()
        {
            try
            {
                IEnumerable<int> siteIds = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
                if (siteIds.Count() == 0)
                    return Ok();
                List<MonthlyAccumchgdhg> datas = new List<MonthlyAccumchgdhg>();
                var keys = CommonFactory.SearchKeys(_redisConn, CommonFactory.PVRedisKeyPattern);

                double total_energy_power = 0;
                double total_chg = 0;
                double total_dhg = 0;

                double smp_land = 0;
                double smp_jeju = 0;

                using (var kpxSession = kpxDataContext.SessionFactory.OpenStatelessSession())
                {
                    var smp_land_row = await kpxSession.GetAsync<HourlySmpLand>(DateTime.Now.Date);

                    var smp_jeju_row = await kpxSession.GetAsync<HourlySmpJeju>(DateTime.Now.Date);


                    smp_land = smp_land_row != null ? smp_land_row.WeightedAverageSmp : 0;
                    smp_jeju = smp_jeju_row != null ? smp_jeju_row.WeightedAverageSmp : 0;


                }

                using (var session = _peiuGridDataContext.SessionFactory.OpenStatelessSession())
                {
                    IList<MonthlyAccumchgdhg> results = await session.CreateCriteria<MonthlyAccumchgdhg>()
                        .Add(Restrictions.InG<int>("SiteId", siteIds))
                        .ListAsync<MonthlyAccumchgdhg>();
                    total_chg = results.Sum(x => x.Charging);
                    total_dhg = results.Sum(x => x.Discharging);


                    var pvresults = await session.CreateCriteria<MonthlyAccumPv>()
                        .Add(Restrictions.InG<int>("SiteId", siteIds))
                        .ListAsync<MonthlyAccumPv>();
                    total_energy_power += pvresults.Sum(x => x.Accumpvpower);
                   


                    //    var sites = _accountContext.VwContractorsites.Where(x => x.UserId == userId).Select(x=>x.SiteId);
                    //    var result = await session.CreateCriteria<TodayAccumchgdhg>().Add(Restrictions.InG<int>("SiteId", sites)).ListAsync<TodayAccumchgdhg>();
                    //    string userId = _claimsManager.GetClaimsValue(HttpContext.User, ClaimTypes.NameIdentifier);
                    //    source = _accountContext.VwContractorusers.Where(x => x.UserId == userId);
                    //}
                    //else if (HttpContext.User.IsInRole(UserRoleTypes.Aggregator))
                    //{
                    //    string groupId = _claimsManager.GetClaimsValue(HttpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                    //    source = _accountContext.VwContractorusers.Where(x => x.AIEggGroupId == groupId);
                    //}

                    return Ok(new { todayactivepowerresult = results, todaypvpowerresult = pvresults, total_accum_charging = total_chg, total_accum_discharging = total_dhg, total_accum_pv_energy = total_energy_power, hourly_wgh_avg_smp_land = smp_land, hourly_wgh_avg_smp_jeju = smp_jeju });
                }
                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex);
            }
        }

        private Func<VwContractorsiteEF, string> MakeClusterKey(int lawCodeLevel)
        {
            switch(lawCodeLevel)
            {
                case 3:
                    return new Func<VwContractorsiteEF, string>(x => x.LawFirstCode + x.LawMiddleCode + x.LawLastCode);
                case 2:
                    return new Func<VwContractorsiteEF, string>(x => x.LawFirstCode + x.LawMiddleCode);
                default:
                    return new Func<VwContractorsiteEF, string>(x => x.LawFirstCode);
            }
        }

        /// <summary>
        /// 1시간 이전 분당 충/방전
        /// </summary>
        /// <param name="siteId">사이트 ID</param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("gehistoricalinanhouractpower")]
        public async Task<IActionResult> GeHistoricalInAnHourActPower(int siteId)
        {
            try
            {
                var avaliable_siteids = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
                if (avaliable_siteids.Contains(siteId) == false)
                {
                    return BadRequest(StatusCodes.Status401Unauthorized);
                }
                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {

                    var pc_result = await MethodsExecutor.GetMinuteStatBySiteId(session, DateTime.Now.AddHours(-1), DateTime.Now, new int[] { siteId });
                    return Ok(pc_result);
                }
            }
            catch(Exception ex)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// 현재 시간 RCC별 PV 출력값 가져오기
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getminutehistoricalpvgrouprcc")]
        public async Task<IActionResult> GetMinuteHistoricalPvGroupRcc()
        {
            try
            {
                var rcc_groups = ControlHelper.GetAvaliableRccCodes(_accountContext, claimsManager, HttpContext);
                DateTime startDate = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
                List<object> result = new List<object>();
                foreach (var avaliable_siteids in rcc_groups)
                {
                    int rcc_code = avaliable_siteids.Key;
                    using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                    {
                        
                        var result_pv = await MethodsExecutor.GetMinutepvStatBySiteId(session, startDate, DateTime.Now, avaliable_siteids);
                        List<DateTime> stamps = new List<DateTime>();
                        List<double> pvsum = new List<double>();
                        var result_group = result_pv.GroupBy(x => x.Timestamp, v => v);
                        foreach(var row in result_group)
                        {
                            stamps.Add(row.Key);
                            pvsum.Add(row.Where(x => x.Accumpvpower.HasValue).Sum(x => x.Accumpvpower.Value));
                        }

                        result.Add(new { rcc = rcc_code, timestamp = stamps, values = pvsum });
                    }
                }
                return Ok(result);
                
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// 1시간 이전 분당 발전데이터
        /// </summary>
        /// <param name="siteId">사이트 ID</param>
        /// <returns></returns>

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("gehistoricalinanhourpvpower")]
        public async Task<IActionResult> GeHistoricalInAnHourPVPower(int siteId)
        {
            try
            {
                var avaliable_siteids = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
                if (avaliable_siteids.Contains(siteId) == false)
                {
                    return BadRequest(StatusCodes.Status401Unauthorized);
                }
                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    var result = await MethodsExecutor.GetMinutepvStatBySiteId(session, DateTime.Now.AddHours(-1), DateTime.Now, new int[] { siteId });
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }


        /// <summary>
        /// 1시간 이전 분당 SOC 평균
        /// </summary>
        /// <param name="siteId">사이트 ID</param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("gehistoricalinanhouravgsoc")]
        public async Task<IActionResult> GeHistoricalInAnHourAvgSoc(int siteId)
        {
            try
            {
                var avaliable_siteids = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
                if (avaliable_siteids.Contains(siteId) == false)
                {
                    return BadRequest(StatusCodes.Status401Unauthorized);
                }
                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    var result = await MethodsExecutor.GetMinuteBmsStatBySiteid(session, DateTime.Now.AddHours(-1), DateTime.Now, new int[] { siteId });
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }



        /// <summary>
        /// PV 예측 및 실측값 가져오기 (1시간 단위) SiteId 조건
        /// </summary>
        /// <param name="startDate">시작날짜</param>
        /// <param name="endDate">종료날짜</param>
        /// <param name="siteId">사이트아이디</param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getpvpredictionbysiteid")]
        public async Task<IActionResult> GetPvPredictionBySiteId(DateTime startDate, DateTime endDate, int siteId)
        {
            try
            {
                JObject result = await GetPvPrediction(startDate, endDate, new int[] { siteId });
                if (result != null)
                    return Ok(result);
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        private async Task<JObject> GetPvPrediction(DateTime startDate, DateTime endDate, IEnumerable<int> siteids)
        {
            try
            {
                JObject result = new JObject();

                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    var pv_result = await MethodsExecutor.GetHoulrlypvStatBySiteId(session, startDate, endDate, siteids);

                    var prediction_result = await session.CreateCriteria<PVPredictionResult>()
                        .Add(Restrictions.InG<int>("SiteId", siteids))
                        .Add(Restrictions.Between("CreateDT", startDate, endDate))
                        .ListAsync<PVPredictionResult>();

                    var result_query = from pv in pv_result
                                       join pre in prediction_result
                                       on new { condition1 = pv.Timestamp, condition2 = pv.SiteId } 
                                       equals new { condition1 = pre.CreateDT, condition2 = pre.SiteId }
                                       select new
                                       {
                                           Timestamp = pv.Timestamp,
                                           siteid = pv.SiteId,
                                           PvPower = pv.Accumpvpower,
                                           PredictPvPower = pre.PVGen
                                       };


                    var prediction_group_hourly = result_query.GroupBy(key => key.Timestamp, value => value);
                    JArray timestamps = new JArray();
                    JArray pv_Values = new JArray();
                    JArray prepv_values = new JArray();
                    foreach (var sub_row in prediction_group_hourly)
                    {
                        timestamps.Add(sub_row.Key);
                        pv_Values.Add(sub_row.Sum(x => x.PvPower));
                        prepv_values.Add(sub_row.Sum(x => x.PredictPvPower));
                    }
                    result.Add("timestamps", timestamps);
                    result.Add("pv_trends", pv_Values);
                    result.Add("predict_pv_trends", prepv_values);
                    result.Add("siteids", JArray.FromObject(siteids));
                }
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// PV 예측 (1시간 단위) 전체 가져오기 (rcc 그룹)
        /// </summary>
        /// <param name="date">요청날짜</param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getpvpredictionresult")]
        public async Task<IActionResult> GetPvPredictionResult(DateTime date)
        {
            try
            {

                var rcc_groups = ControlHelper.GetAvaliableRccCodes(_accountContext, claimsManager, HttpContext);
                DateTime endDate = date.Date.AddHours(23);
                JArray result = new JArray();
                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    foreach (var row in rcc_groups)
                    {
                        var prediction_result = await session.CreateCriteria<PVPredictionResult>()
                            .Add(Restrictions.InG<int>("SiteId", row))
                            .Add(Restrictions.Between("CreateDT", date, endDate))
                            .ListAsync<PVPredictionResult>();

                        var prediction_group_hourly = prediction_result.GroupBy(key => key.CreateDT, value => value);
                        JObject rcc_row = new JObject();
                        rcc_row.Add("rcc", row.Key);
                        rcc_row.Add("siteids", new JArray( row));
                        JArray data_Row = new JArray();
                        JArray time_row = new JArray();
                        foreach (var sub_row in prediction_group_hourly)
                        {
                            data_Row.Add(sub_row.Sum(x => x.PVGen));
                            time_row.Add(sub_row.Key);
                        }
                        rcc_row.Add("predict_pv", data_Row);
                        rcc_row.Add("timestamp", time_row);
                        result.Add(rcc_row);
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// PV 예측 및 실측값 가져오기 (1시간 단위) rcc 조건
        /// </summary>
        /// <param name="startDate">시작날짜</param>
        /// <param name="endDate">종료날짜</param>
        /// <param name="rcc">rcc번호</param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getpvpredictionbyrcc")]
        public async Task<IActionResult> GetPvPredictionByRcc(DateTime startDate, DateTime endDate, int rcc)
        {
            try
            {

                IEnumerable<int> siteIds_group = null;
                siteIds_group = ControlHelper.GetAvaliableSiteIdsByRcc(_accountContext, claimsManager, HttpContext, rcc);

                JObject result = await GetPvPrediction(startDate, endDate, siteIds_group);
                if (result != null)
                    return Ok(result);
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// PV 예측 및 실측값 가져오기 (1시간 단위) aggid 조건
        /// </summary>
        /// <param name="startDate">시작날짜</param>
        /// <param name="endDate">종료날짜</param>
        /// <param name="AggId">어그리게이터 ID</param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.OnlySupervisor)]
        [HttpGet, Route("getpvpredictionbyaggid")]
        public async Task<IActionResult> GetPvPredictionByAggId(DateTime startDate, DateTime endDate, string AggId)
        {
            try
            {

                IEnumerable<int> siteIds_group = null;
                siteIds_group = ControlHelper.GetAvaliableSiteIdsByAggGroupId(_accountContext, claimsManager, HttpContext, AggId);
                JObject result = await GetPvPrediction(startDate, endDate, siteIds_group);
                if (result != null)
                    return Ok(result);
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// 법정동 레벨별 충/방전, PV 발전, SOC 요청 (실시간 데이터)
        /// </summary>
        /// <param name="lawcodelevel">법정동 코드 레벨</param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getstatisticscurrentvalue")]
        public async Task<IActionResult> GetStatisticsCurrentValue(int lawcodelevel = 1)
        {
            JObject row_j = null;
            try
            {
                
                IEnumerable<VwContractorsiteEF> sites = ControlHelper.GetAvaliableSites(_accountContext, claimsManager, HttpContext);
                if (sites.Count() == 0)
                    return Ok();

                var group_sites = sites.GroupBy(MakeClusterKey(lawcodelevel), value => value);
                IEnumerable<int> siteIds = sites.Select(x => x.SiteId);

                JArray result = new JArray();
                List<double> total_socs = new List<double>();
                using (var session = _peiuGridDataContext.SessionFactory.OpenStatelessSession())
                {
                    bool IsUpdate = lastUpdateSiteIdFromRedis < DateTime.Now;
                    foreach (IGrouping<string, VwContractorsiteEF> row in group_sites)
                    {
                        double total_energy_power = 0;
                        double total_actPwr_charging = 0;
                        double total_actPwr_discharging = 0;
                        int event_count = 0;
                        List<double> socs = new List<double>();
                        List<double> sohs = new List<double>();
                        int first_siteid = -1;

                        // Event Count
                        var query_results = await session.CreateCriteria<VwEventRecord>()
                   .Add((Restrictions.IsNull("AckDT") || Restrictions.IsNull("RecoveryDT")) && Restrictions.InG<int>("SiteId", row.Select(x=>x.SiteId)))
                   .ListAsync<VwEventRecord>();
                        event_count = query_results.Count;

                        foreach (VwContractorsiteEF site in row)
                        {
                            if (first_siteid == -1)
                                first_siteid = site.SiteId;
                            // PV 
                            string target_redis_key = CommonFactory.CreateRedisKey(site.SiteId, 4, "PV*");
                            if (IsUpdate || siteIdKeys.ContainsKey(target_redis_key) == false)
                            {
                                siteIdKeys[target_redis_key] = CommonFactory.SearchKeys(_redisConn, target_redis_key);
                            }
                            var redis_keys = siteIdKeys[target_redis_key];
                            foreach (RedisKey pv_key in redis_keys)
                            {
                                double TotalActivePower = (double)await _redisDb.HashGetAsync(pv_key, "TotalActivePower");
                                total_energy_power += TotalActivePower;
                            }

                            // PCS
                            target_redis_key = CommonFactory.CreateRedisKey(site.SiteId, 1, "PCS*");
                            if (IsUpdate || siteIdKeys.ContainsKey(target_redis_key) == false)
                            {
                                siteIdKeys[target_redis_key] = CommonFactory.SearchKeys(_redisConn, target_redis_key);
                            }
                            redis_keys = siteIdKeys[target_redis_key];
                            foreach (RedisKey key in redis_keys)
                            {
                                double TotalActivePower = (double)await _redisDb.HashGetAsync(key, "actPwrKw");
                                if (TotalActivePower > 0)
                                    total_actPwr_discharging += TotalActivePower;
                                else
                                    total_actPwr_charging += TotalActivePower;

                            }

                            // BMS
                            target_redis_key = CommonFactory.CreateRedisKey(site.SiteId, 2, "BMS*");
                            if (IsUpdate || siteIdKeys.ContainsKey(target_redis_key) == false)
                            {
                                siteIdKeys[target_redis_key] = CommonFactory.SearchKeys(_redisConn, target_redis_key);
                            }
                            redis_keys = siteIdKeys[target_redis_key];
                            foreach (RedisKey key in redis_keys)
                            {
                                double soc = (double)await _redisDb.HashGetAsync(key, "bms_soc");
                                socs.Add(soc);
                                total_socs.Add(soc);
                                double soh = (double)await _redisDb.HashGetAsync(key, "bms_soh");
                                sohs.Add(soh);
                            }

                           


                        }
                        if(IsUpdate)
                        {
                            lastUpdateSiteIdFromRedis = DateTime.Now.AddMinutes(15);
                        }
                        JObject weather_obj = new JObject();
                        if (row.Count() > 0)
                        {
                            // Weather
                            weather_obj = await CommonFactory.RetriveWeather(row.FirstOrDefault().SiteId, _redisDb);
                        }

                        row_j = new JObject();
                        row_j.Add("LawCode", row.Key);
                        row_j.Add("total_pvpower", total_energy_power);
                        row_j.Add("total_charging", Math.Abs(total_actPwr_charging));
                        row_j.Add("total_discharging", total_actPwr_discharging);
                        row_j.Add("average_soc", socs.Count() > 0 ? socs.Average() : 0);
                        row_j.Add("average_soh", sohs.Count() > 0 ? sohs.Average() : 0);
                        row_j.Add("total_count", row.Count());
                        row_j.Add("event_count", event_count);
                        row_j.Add("weather", weather_obj);
                        result.Add(row_j);
                    }
                }

                JObject center_weather = await CommonFactory.RetriveWeather(0, _redisDb);

                double total_socs_avg = total_socs.Count > 0 ? total_socs.Average() : 0;

                return Ok(new { group = result, total_average_soc = total_socs_avg, total_event_count = 0, total_sites_count = siteIds.Count(), controlcenter_weather = center_weather });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }



        private async Task<List<double>> ReadSiteStat(int siteId, int groupNo, string filter, string field)
        {
            List<double> result = new List<double>();
            string target_redis_key = CommonFactory.CreateRedisKey(siteId, groupNo, filter);
            var redis_keys = CommonFactory.SearchKeys(_redisConn, target_redis_key);
            foreach (RedisKey pv_key in redis_keys)
            {
                double TotalActivePower = (double)await _redisDb.HashGetAsync(pv_key, field);
                result.Add(TotalActivePower);
            }
            return result;
        }

        /// <summary>
        /// 사이트별 현재 날씨 가져오기
        /// </summary>
        /// <param name="siteId">사이트 ID</param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getweatherbysiteid")]
        public async Task<IActionResult> GetWeatherBySiteId(int siteId)
        {
            JObject weather = await CommonFactory.RetriveWeather(siteId, _redisDb);
            if (weather == null)
                return BadRequest();
            else
                return Ok(weather);
        }



        /// <summary>
        /// 사이트별 현재 ActPwr, SOC, PV출력
        /// </summary>
        /// <param name="siteId">사이트 ID</param>
        /// <returns>ActPwr, SOC, PV출력</returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getcontractassetbyfindsiteid")]
        public async Task<IActionResult> GetContractassetbyFindsiteid(int siteId)
        {
            IEnumerable<int> siteIds = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
            if (siteIds.Contains(siteId) == false)
                return BadRequest();

            List<double> total_actpower = await ReadSiteStat(siteId, 1, "PCS*", "actPwrKw");
            List<double> total_soc = await ReadSiteStat(siteId, 2, "BMS*", "bms_soc");
            List<double> total_pvpower = await ReadSiteStat(siteId, 4, "PV*", "TotalActivePower");

            return Ok(new { activepower = total_actpower.Sum(), soc = total_soc.Count() > 0 ? total_soc.Average() : 0, pvpower = total_pvpower.Sum() });

        }

        /// <summary>
        /// 금일 충/방전, PV발전 누적데이터
        /// </summary>
        /// <returns>충/방전, PV발전량</returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("gettodayaccumuactivepower")]
        public async Task<IActionResult> GetTodayAccumuActivePower()
        {
            try
            {
                IEnumerable<int> siteIds = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
                if (siteIds.Count() == 0)
                    return Ok();
                List<TodayAccumchgdhg> datas = new List<TodayAccumchgdhg>();
                //if (HttpContext.User.IsInRole(UserRoleTypes.Supervisor))
                //{
                //    siteIds = _accountContext.VwContractorsites.Select(x => x.SiteId);
                //    //string key = $"Supervisor.Statistics.H{DateTime.Now.Hour}";
                //    //if (await _redisDb.HashExistsAsync(key, "chg") && await _redisDb.HashExistsAsync(key, "dhg"))
                //    //    return
                //    // datas.AddRange(await session.CreateCriteria<TodayAccumchgdhg>().ListAsync<TodayAccumchgdhg>());
                //}
                //else if (HttpContext.User.IsInRole(UserRoleTypes.Contractor))
                //{
                //    string userId = _claimsManager.GetClaimsValue(HttpContext.User, ClaimTypes.NameIdentifier);
                //    siteIds = _accountContext.VwContractorsites.Where(x => x.UserId == userId).Select(x => x.SiteId);
                //}
                //else if (HttpContext.User.IsInRole(UserRoleTypes.Aggregator))
                //{
                //    string groupId = _claimsManager.GetClaimsValue(HttpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                //    siteIds = _accountContext.VwContractorsites.Where(x => x.AggGroupId == groupId).Select(x => x.SiteId);
                //}

                var keys = CommonFactory.SearchKeys(_redisConn, CommonFactory.PVRedisKeyPattern);

                double total_energy_power = 0;

               
                double total_chg = 0;
                double total_dhg = 0;
                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    IList< TodayAccumchgdhg> results = await session.CreateCriteria<TodayAccumchgdhg>()
                        .Add(Restrictions.InG<int>("SiteId", siteIds))
                        .ListAsync<TodayAccumchgdhg>();
                    total_chg = results.Sum(x => x.Charging);
                    total_dhg = results.Sum(x => x.Discharging);

                    var pvresults = await session.CreateCriteria<TodayAccumPv>()
                       .Add(Restrictions.InG<int>("SiteId", siteIds))
                       .ListAsync<TodayAccumPv>();
                    total_energy_power += pvresults.Sum(x => x.Accumpvpower);

                    //    var sites = _accountContext.VwContractorsites.Where(x => x.UserId == userId).Select(x=>x.SiteId);
                    //    var result = await session.CreateCriteria<TodayAccumchgdhg>().Add(Restrictions.InG<int>("SiteId", sites)).ListAsync<TodayAccumchgdhg>();
                    //    string userId = _claimsManager.GetClaimsValue(HttpContext.User, ClaimTypes.NameIdentifier);
                    //    source = _accountContext.VwContractorusers.Where(x => x.UserId == userId);
                    //}
                    //else if (HttpContext.User.IsInRole(UserRoleTypes.Aggregator))
                    //{
                    //    string groupId = _claimsManager.GetClaimsValue(HttpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                    //    source = _accountContext.VwContractorusers.Where(x => x.AggGroupId == groupId);
                    //}

                    return Ok(new { todayactivepowerresult = results, todaypvpowerresult = pvresults, toady_accum_charging = total_chg, today_accum_discharging = total_dhg, today_accum_pv_energy = total_energy_power });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //[Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        //[HttpGet, Route("gethourlyrevenuebyrcc")]
        //public async Task<IActionResult> GetHourlyRevenueByRcc(int rcc = -1)
        //{
        //    try
        //    {

        //        var avaliable_siteids = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
        //        if (siteId != -1 && avaliable_siteids.Contains(siteId) == false)
        //        {
        //            return BadRequest(StatusCodes.Status401Unauthorized);
        //        }

        //        AbstractCriterion ingFilter = null;

        //        if (siteId == -1)
        //        {
        //            ingFilter = Restrictions.InG<int>("SiteId", avaliable_siteids);
        //        }
        //        else''
        //            ingFilter = Restrictions.Eq("SiteId", siteId);

        //        using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
        //        {
        //            var result = await session.CreateCriteria<Hourlyrevenue>()
        //                .Add(Restrictions.Eq("Timestamp", DateTime.Now.Date) && Restrictions.Eq("Hour", DateTime.Now.Hour) && ingFilter)
        //                .ListAsync<Hourlyrevenue>();
        //            return Ok(result);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, ex.Message);
        //        return BadRequest(ex.Message);
        //    }
        //}
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getlatestsitestatus")]
        public async Task<IActionResult> GetLatestSiteStatus(int siteId)
        {
            try
            {

                //CALL `grid`.`CalcRevenueHourlyByDate`(<{in target_date datetime}>);

                var avaliable_siteids = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
                if (siteId != -1 && avaliable_siteids.Contains(siteId) == false)
                {
                    return BadRequest(StatusCodes.Status401Unauthorized);
                }

                //AbstractCriterion ingFilter = Restrictions.Eq("SiteId", siteId);



                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    var result = await session.CreateSQLQuery($" `grid`.`GetLatestSiteStatus`({siteId})")
                        .UniqueResultAsync();
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 금일 실제 수익금 및 예상 수익금 (전체)
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("gettodayrevenue")]
        public async Task<IActionResult> GetTodayRevenue()
        {
            try
            {
                DateTime date = DateTime.Now;

                var site_ids = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
                //CALL `grid`.`CalcRevenueHourlyByDate`(<{in target_date datetime}>);
                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    var Today_revenue = await session.CreateCriteria<Hourlyrevenue>()
                       .Add(Restrictions.InG<int>("SiteId", site_ids) && Restrictions.Eq("Timestamp", date.Date))
                       //.Add(Restrictions.Where<Hourlyrevenue>(x=>x.Timestamp.Date == date.Date))
                       .AddOrder(NHibernate.Criterion.Order.Asc("Hour"))
                       .ListAsync<Hourlyrevenue>();
                    var predict_rev_result = await session.CreateCriteria<PredictRevenue>()
                        .Add(Restrictions.InG<int>("SiteId", site_ids) && Restrictions.Eq("Timestamp", date.Date))
                        .ListAsync<PredictRevenue>();
                    return Ok(new { /*results = monthly_result,*/ TodayRevenue = Today_revenue.Sum(x=>x.Money), TodayPredictRevenue = predict_rev_result.Sum(x=>x.TotalRevenue) });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 금일 실제 수익금 및 예상 수익금 및 고장발생 건수 (사이트별)
        /// </summary>
        /// <param name="date">날짜</param>
        /// <param name="siteId">사이트 ID</param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getdailyrevenuebysiteid")]
        public async Task<IActionResult> GetDailyRevenueBySiteId(DateTime date, int siteId)
        {
            try
            {

                //CALL `grid`.`CalcRevenueHourlyByDate`(<{in target_date datetime}>);
                if (date == default(DateTime))
                    date = DateTime.Now;
                var avaliable_siteids = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
                if (siteId != -1 && avaliable_siteids.Contains(siteId) == false)
                {
                    return BadRequest(StatusCodes.Status401Unauthorized);
                }


                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    var monthly_result = await session.CreateCriteria<Hourlyrevenue>()
                       .Add(Restrictions.Eq("SiteId", siteId) && Restrictions.Eq("Timestamp", date.Date))
                       //.Add(Restrictions.Where<Hourlyrevenue>(x=>x.Timestamp.Date == date.Date))
                       .AddOrder(NHibernate.Criterion.Order.Asc("Hour"))
                       .ListAsync<Hourlyrevenue>();
                    var event_result = await session.CreateCriteria<EventRecord>()
                        .Add(Restrictions.Eq("SiteId", siteId) &&
                        Restrictions.IsNotNull("RecoveryDT") && Restrictions.IsNotNull("AckDT"))
                        .Add(Restrictions.Where<EventRecord>(x => x.CreateDT.Date == date.Date))
                        .ListAsync<EventRecord>();
                    var predict_rev_result = await session.CreateCriteria<PredictRevenue>()
                        .Add(Restrictions.Eq("SiteId", siteId) && Restrictions.Eq("Timestamp", date.Date))
                        .UniqueResultAsync<PredictRevenue>();
                    return Ok(new { /*results = monthly_result,*/ Revenue = monthly_result.LastOrDefault(), RevenueHistory = monthly_result, CountOfEvent = event_result.Count, PredictRevenue = predict_rev_result?.TotalRevenue });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 금월 누적 수익금
        /// </summary>
        /// <param name="siteId">사이트 ID</param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getmonthlyrevenuebysiteid")]
        public async Task<IActionResult> GetMonthlyRevenueBySiteId(int siteId = -1)
        {
            try
            {

                //CALL `grid`.`CalcRevenueHourlyByDate`(<{in target_date datetime}>);

                var avaliable_siteids = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
                if (siteId != -1 && avaliable_siteids.Contains(siteId) == false)
                {
                    return BadRequest(StatusCodes.Status401Unauthorized);
                }

                AbstractCriterion ingFilter = null;

                if (siteId == -1)
                {
                    ingFilter = Restrictions.InG<int>("SiteId", avaliable_siteids);
                }
                else
                    ingFilter = Restrictions.Eq("SiteId", siteId);

                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    
                     var monthly_result = await session.CreateCriteria<Hourlyrevenue>()
                        .Add(Restrictions.Where<Hourlyrevenue>(x=>x.Timestamp.Year == DateTime.Now.Year && x.Timestamp.Month == DateTime.Now.Month) )
                        //.Add(Restrictions.Eq("Hour", 23))
                        .Add(ingFilter)
                        .ListAsync<Hourlyrevenue>();
                    double money = monthly_result.Sum(x => x.Money);
                    return Ok(new { /*results = monthly_result,*/ summary = money });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 마켓 데이터 가져오기 (AGG그룹별)
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.RequiredManager)]
        [HttpGet, Route("getmarketdatabyaggid")]
        public async Task<IActionResult> GetMarketDataByAggId(string aggid)
        {
            try
            {
                IEnumerable<int> avaliable_siteids = ControlHelper.GetAvaliableSiteIdsByAggGroupId(_accountContext, claimsManager, HttpContext, aggid);

                List<Object> data = new List<object>();
                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    var today_rev_result = await session.CreateCriteria<Hourlyrevenue>()
                   .Add(Restrictions.Eq("Timestamp", DateTime.Now.Date)  && Restrictions.InG<int>("SiteId", avaliable_siteids))
                   .AddOrder(NHibernate.Criterion.Order.Desc("Timestamp"))
                   .ListAsync<Hourlyrevenue>();

                    var pv_result = await MethodsExecutor.GetMinutepvStatBySiteId(session, DateTime.Now.Date, DateTime.Now, avaliable_siteids);

                    var pv_group_by_siteid = pv_result.GroupBy(x => x.SiteId, v => v);

                    foreach(var row in pv_group_by_siteid)
                    {
                        int siteid = row.Key;
                        float today_money = today_rev_result.Where(x=>x.SiteId == siteid).Sum(x => x.Money);
                        double pvpower = row.Where(x=>x.Accumpvpower.HasValue).Sum(x => x.Accumpvpower.Value);
                        data.Add(new { siteId = siteid, money = today_money, pvpower = pvpower / 60 });
                    }
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }

        }

        /// <summary>
        /// 마켓 데이터 가져오기 (RCC별)
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getmarketdata")]
        public async Task<IActionResult> GetMarketData()
        {
            try
            {
                List<Object> data = new List<object>();
                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    var group_rccs = ControlHelper.GetAvaliableRccCodes(_accountContext, claimsManager, HttpContext);
                    DateTime now = DateTime.Now.AddHours(-1);
                    foreach(var row in group_rccs)
                    {
                        int rcc_code = row.Key;
                        var site_ids = row.Select(x => x);
                        var today_rev_result = await session.CreateCriteria<Hourlyrevenue>()
                       .Add(Restrictions.Eq("Timestamp", now.Date) &&  Restrictions.InG<int>("SiteId", site_ids))
                       .AddOrder(NHibernate.Criterion.Order.Desc("Timestamp"))
                       .ListAsync<Hourlyrevenue>();

                        var pv_result = await MethodsExecutor.GetMinutepvStatByRcc(session, DateTime.Now.Date, DateTime.Now, rcc_code);
                        float today_money = today_rev_result.Sum(x => x.Money);
                        double pvpower = pv_result.Sum(x => x.Accumpvpower.Value);
                        data.Add(new { rcc = rcc_code, money = today_money, pvpower = pvpower / 60,countOfSites = site_ids.Count() });

                    }
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("gethourlyrevenuebysiteid")]
        public async Task<IActionResult> GetHourlyRevenueBySiteId(int siteId = -1)
        {
            try
            { 
                IEnumerable<int> siteids = new int[] { siteId };
                var avaliable_siteids = ControlHelper.GetAvaliableSiteIds(_accountContext, claimsManager, HttpContext);
                if (siteId != -1 && avaliable_siteids.Contains(siteId) == false)
                {
                    return BadRequest(StatusCodes.Status401Unauthorized);
                }

                AbstractCriterion ingFilter = null;

                if (siteId == -1)
                {
                    siteids = avaliable_siteids;
                    ingFilter = Restrictions.InG<int>("SiteId", avaliable_siteids);
                }
                else
                    ingFilter = Restrictions.Eq("SiteId", siteId);

                DateTime startDt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime yesterDt = DateTime.Now.Date.AddSeconds(-1);
                DateTime endDt = DateTime.Now.Date.AddHours(DateTime.Now.Hour);

                using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
                {
                    var monthly_rev_result = await session.CreateCriteria<Hourlyrevenue>()
                        .Add(Restrictions.Between("Timestamp", startDt, yesterDt) && ingFilter)
                        .ListAsync<Hourlyrevenue>();

                    var today_rev_result = await session.CreateCriteria<Hourlyrevenue>()
                        .Add(Restrictions.Eq("Timestamp", endDt.Date)  && ingFilter)
                        .AddOrder(NHibernate.Criterion.Order.Desc("Timestamp"))
                        .ListAsync<Hourlyrevenue>();

                    var event_result = await session.CreateCriteria<VwEventRecord>()
                        .Add(Restrictions.Between("CreateDT", startDt, DateTime.Now))
                        .Add(Restrictions.InG<int>("SiteId", avaliable_siteids))
                        .ListAsync<VwEventRecord>();

                    Hourlyrevenue today_rev = today_rev_result.FirstOrDefault();
                    float today_money = (today_rev != null ? today_rev.Money : 0f);
                    float monthly_rev_sum = monthly_rev_result.Sum(x => x.Money) + today_money;
                    int today_event_count = event_result.Count(x => x.CreateDT.Date == DateTime.Now.Date);
                    int month_event_count = event_result.Count();
                        return Ok(new { TodayRevenue = today_money, MonthlyRevenue = monthly_rev_sum, TodayEventCount = today_event_count, monthly_event_count = month_event_count });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }


                //[HttpGet("downloadhistorybyrcc")]
                //public async Task<IActionResult> downloadhistorybyrcc(int rcc, DateTime startdate, DateTime enddate)
                //{

                //    var rcc_map = PeiuPlatform.App.Controllers.PMSController.rcc_list;
                //    if (rcc_map.ContainsKey(rcc) == false)
                //        return BadRequest();
                //    string areaName = rcc_map[rcc];

                //    var rcc_by_site_map = PeiuPlatform.App.Controllers.PMSController.RccBySiteMap;
                //    //converting Pdf file into bytes array  
                //    //adding bytes to memory stream   
                //    //var dataStream = new MemoryStream(dataBytes);

                //    string file = $"{areaName}지역_{DateTime.Now:yyyy-MM-dd HHmmss}.csv";

                //    MongoDB.Driver.IMongoDatabase main_db = mongoClient.GetDatabase("PEIU");
                //    var cols = main_db.GetCollection<BsonDocument>("daegun_meter");
                //    var builder = Builders<BsonDocument>.Filter;
                //    DateTime today = startdate.Date.ToUniversalTime();
                //    DateTime tommorow = enddate.Date.AddDays(1);
                //    var filter = builder.In("sSiteId", rcc_by_site_map[rcc]) & builder.Gte("timestamp", today) & builder.Lt("timestamp", tommorow);
                //    JArray trends = new JArray();
                //    var result = await cols.Find(filter).Sort("{timestamp: 1}").ToListAsync();

                //    List<string> headerStr = new List<string>();
                //    StringBuilder csv_builder = new StringBuilder();
                //    csv_builder.AppendLine("timestamp,siteid,activepower,soc,soh,pvactivepower,pvvoltage,pcscurrent,pcsstatus,freq");
                //    foreach(var db in result)
                //    {
                //        DateTime timeStamp = db["timestamp"].ToUniversalTime();
                //        DateTime localTime = timeStamp.ToLocalTime();

                //        object[] row_datas = new object[]
                //        {
                //            localTime.ToString("yyyy-MM-dd HH:mm:ss"),
                //            db["sSiteId"].AsInt32,
                //            db["Pcs"]["ActivePower"].AsDouble,
                //            db["Bsc"]["Soc"].AsDouble,
                //            db["Bsc"]["Soh"].AsDouble,
                //            db["Pv"]["TotalActivePower"].AsDouble,
                //            db["Pv"]["Voltage"]["R"].AsDouble,
                //            db["Pcs"]["AC_phase_current"]["R"].AsDouble,
                //            db["Pcs"]["Status"].AsInt32,
                //            db["Ess"]["Frequency"].AsDouble
                //        };
                //        csv_builder.AppendLine(string.Join(",", row_datas));
                //    }

                //    string str = csv_builder.ToString();
                //    byte[] dataBytes = Encoding.UTF8.GetBytes(str);
                //    // Response...
                //    System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
                //    {
                //        FileName = file,
                //        Inline = false  // false = prompt the user for downloading;  true = browser to try to show the file inline
                //    };
                //    Response.Headers.Add("Content-Disposition", cd.ToString());
                //    Response.Headers.Add("X-Content-Type-Options", "nosniff");
                //    return File(dataBytes, "application/octet-stream");

                //    //return new FileDownloadAction(dataStream, Request, $"EMS_{DateTime.Now:yyyy-MM-dd HHmmss}.raw");
                //    //return new eBookResult(dataStream, Request, bookName);
                //}


                //[HttpGet("gettrendinfo")]
                //public async Task<IActionResult> gettrenddata(int[] siteid, DateTime startdate, DateTime enddate, int PageNo, int ShowRowCount, int SortNum = -1)
                //{
                //    MongoDB.Driver.IMongoDatabase main_db = mongoClient.GetDatabase("PEIU");
                //    var cols = main_db.GetCollection<BsonDocument>("daegun_meter");
                //    var builder = Builders<BsonDocument>.Filter;
                //    DateTime today = startdate.Date.ToUniversalTime();
                //    DateTime tommorow = enddate.Date.AddDays(1);
                //    var filter = builder.In("sSiteId", siteid) & builder.Gte("timestamp", today) & builder.Lt("timestamp", tommorow);
                //    JArray trends = new JArray();
                //    await cols.Find(filter).Limit(ShowRowCount).Skip((PageNo - 1) * ShowRowCount).Sort("{timestamp: " + SortNum + "}").ForEachAsync(
                //        db =>
                //        {
                //            DateTime timeStamp = db["timestamp"].ToUniversalTime();
                //            DateTime localTime = timeStamp.ToLocalTime();

                //            JObject trend_data = new JObject();
                //            trend_data.Add("time", localTime);
                //            trend_data.Add("siteid", db["sSiteId"].AsInt32);

                //            trend_data.Add("ActivePower", db["Pcs"]["ActivePower"].AsDouble);
                //            trend_data.Add("Soc", db["Bsc"]["Soc"].AsDouble);
                //            trend_data.Add("Soh", db["Bsc"]["Soh"].AsDouble);
                //            trend_data.Add("PvActivePower", db["Pv"]["TotalActivePower"].AsDouble);
                //            trend_data.Add("pvVoltage", db["Pv"]["Voltage"]["R"].AsDouble);
                //            trend_data.Add("ACCurrent", db["Pcs"]["AC_phase_current"]["R"].AsDouble);
                //            trend_data.Add("pcs_status", db["Pcs"]["Status"].AsInt32);
                //            trend_data.Add("Frequency", db["Ess"]["Frequency"].AsDouble);
                //            trends.Add(trend_data);
                //        }
                //        );
                //    return Ok(trends);
                //    //db.getCollection('daegun_meter').find({ sSiteId: 148}).skip((4 - 1) * 10).limit(10)
                //}

                //[HttpGet("gettrenddatabyrcc")]
                //public async Task<IActionResult> gettrenddatabyrcc(int rccCode, DateTime date)
                //{
                //    var rcc_map = PeiuPlatform.App.Controllers.PMSController.RccBySiteMap;
                //    if (rcc_map.ContainsKey(rccCode) == false)
                //        return BadRequest();

                //    MongoDB.Driver.IMongoDatabase main_db = mongoClient.GetDatabase("PEIU");
                //    var cols = main_db.GetCollection<BsonDocument>("daegun_meter");
                //    var builder = Builders<BsonDocument>.Filter;
                //    DateTime today = date.Date.ToUniversalTime();
                //    DateTime tommorow = today.AddDays(1);
                //    var filter = builder.In("sSiteId", rcc_map[rccCode]) & builder.Gte("timestamp", today) & builder.Lt("timestamp", tommorow);
                //    var cursor = cols.Find(filter).Sort("{timestamp: -1}");

                //    ConcurrentDictionary<DateTime, List<JObject>> values = new ConcurrentDictionary<DateTime, List<JObject>>();

                //    await cursor.ForEachAsync(db =>
                //    {
                //        DateTime timeStamp = db["timestamp"].ToUniversalTime();
                //        DateTime localTime = timeStamp.ToLocalTime();
                //        DateTime input_time = new DateTime(localTime.Year, localTime.Month, localTime.Day, localTime.Hour, 0, 0);
                //        if(values.ContainsKey(input_time) == false)
                //        {
                //            values[input_time] = new List<JObject>();
                //        }




                //        JObject trend_data = new JObject();
                //        trend_data.Add("time", localTime);
                //        trend_data.Add("siteid", db["sSiteId"].AsInt32);
                //        trend_data.Add("ActivePower", db["Pcs"]["ActivePower"].AsDouble);
                //        trend_data.Add("Soc", db["Bsc"]["Soc"].AsDouble);
                //        trend_data.Add("Soh", db["Bsc"]["Soh"].AsDouble);
                //        trend_data.Add("PvActivePower", db["Pv"]["TotalActivePower"].AsDouble);
                //        trend_data.Add("pvVoltage", db["Pv"]["Voltage"]["R"].AsDouble);
                //        trend_data.Add("ACCurrent", db["Pcs"]["AC_phase_current"]["R"].AsDouble);
                //        trend_data.Add("pcs_status", db["Pcs"]["Status"].AsInt32);
                //        trend_data.Add("Frequency", db["Ess"]["Frequency"].AsDouble);
                //        values[input_time].Add(trend_data);

                //    });

                //    JArray result_array = new JArray();
                //    var keys = values.Keys.OrderBy(x => x);
                //    foreach(DateTime dt in keys)
                //    {
                //        JObject trend_data = new JObject();
                //        trend_data.Add("time", dt);
                //        trend_data.Add("ActivePower", values[dt].Select(x => x["ActivePower"].Value<double>()).Sum());
                //        trend_data.Add("Soc", values[dt].Select(x => x["Soc"].Value<double>()).Average());
                //        trend_data.Add("Soh", values[dt].Select(x => x["Soh"].Value<double>()).Average());
                //        trend_data.Add("PvActivePower", values[dt].Select(x => x["PvActivePower"].Value<double>()).Average());
                //        trend_data.Add("pvVoltage", values[dt].Select(x => x["pvVoltage"].Value<double>()).Average());
                //        trend_data.Add("ACCurrent", values[dt].Select(x => x["ACCurrent"].Value<double>()).Average());
                //        result_array.Add(trend_data);
                //    }

                //    return Ok(result_array);

                //}

                //[HttpGet("gettrenddata")]
                //public async Task<IActionResult> gettrenddata(int siteid, DateTime date)
                //{
                //    MongoDB.Driver.IMongoDatabase main_db = mongoClient.GetDatabase("PEIU");
                //    var cols = main_db.GetCollection<BsonDocument>("daegun_meter");
                //    var builder = Builders<BsonDocument>.Filter;
                //    DateTime today = date.Date.ToUniversalTime();
                //    DateTime tommorow = today.AddDays(1);
                //    var filter = builder.Eq("sSiteId", siteid) & builder.Gte("timestamp", today) & builder.Lt("timestamp", tommorow);
                //    IAsyncCursor<BsonDocument> cursor = await cols.FindAsync(filter);
                //    JArray trends = new JArray();
                //    await cursor.ForEachAsync(db =>
                //    {
                //        DateTime timeStamp = db["timestamp"].ToUniversalTime();
                //        DateTime localTime = timeStamp.ToLocalTime();

                //        JObject trend_data = new JObject();
                //        trend_data.Add("time", localTime);


                //        trend_data.Add("ActivePower", db["Pcs"]["ActivePower"].AsDouble);
                //        trend_data.Add("Soc", db["Bsc"]["Soc"].AsDouble);
                //        trend_data.Add("Soh", db["Bsc"]["Soh"].AsDouble);
                //        trend_data.Add("PvActivePower", db["Pv"]["TotalActivePower"].AsDouble);
                //        trend_data.Add("pvVoltage", db["Pv"]["Voltage"]["R"].AsDouble);
                //        trend_data.Add("ACCurrent", db["Pcs"]["AC_phase_current"]["R"].AsDouble);
                //        trend_data.Add("pcs_status", db["Pcs"]["Status"].AsInt32);
                //        trend_data.Add("Frequency", db["Ess"]["Frequency"].AsDouble);
                //        trends.Add(trend_data);

                //    });

                //    return Ok(trends);

                //}

                //[HttpGet("allstat")]
                //public async Task<IActionResult> GetAllStat()
                //{
                //    UpdateSiteKeyByBsc();
                //    JArray array = new JArray();
                //    foreach(RedisKey bsc_key in AllSiteKeyByBsc)
                //    {
                //        JObject row = new JObject();
                //        string siteId = bsc_key.ToString().Split('_')[1];
                //        string pv_Key = $"site_{siteId}_PVMETER";
                //        string pcs_key = $"site_{siteId}_PCS";

                //        double pcs_activePwr = (double)await _db.HashGetAsync(pcs_key, "ActivePower");
                //        double pv_activePwr = (double)await _db.HashGetAsync(pv_Key, "TotalActivePower");
                //        double soc = (double)await _db.HashGetAsync(bsc_key, "Soc");
                //        row.Add("siteId", siteId);
                //        row.Add("soc_average", soc);
                //        row.Add("pv_activePower", pv_activePwr);
                //        row.Add("pcs_activePower", pcs_activePwr);
                //        array.Add(row);
                //    }

                //    return Ok(array);

                //}
                //[HttpGet("TotalAccumPower")]
                //public async Task<IActionResult> TotalAccumPower()
                //{
                //    UpdateSiteKeyByBsc();
                //    var contracts = mapreduce_db.GetCollection<BsonDocument>("ContractInfo");
                //    var cols = mapreduce_db.GetCollection<BsonDocument>("Statistics");
                //    string filter = $"_id: /^{DateTime.Now.Year}-{DateTime.Now.Month:0#}/";
                //    IAsyncCursor<BsonDocument> cursor = await cols.FindAsync("{" + filter + "}" );
                //    var result = await cursor.ToListAsync();
                //    double total_pv = 0;
                //    double total_chg = 0;
                //    double total_dhg = 0;
                //    double today_pv = 0;
                //    double today_chg = 0;
                //    double today_dhg = 0;
                //    if (result.Count > 0)
                //    {
                //         total_pv = result.Sum(x => x["value"]["pvactpwr"].AsDouble);
                //         total_chg = result.Sum(x => x["value"]["accumchg"].AsDouble);
                //         total_dhg = result.Sum(x => x["value"]["accumdhg"].AsDouble);
                //        string dt = DateTime.Today.ToString("yyyy-MM-dd");
                //        var today_row = result.FirstOrDefault(x => x["_id"].AsString == dt);
                //        if(today_row != null)
                //        {
                //            today_pv = today_row["value"]["pvactpwr"].AsDouble;
                //            today_chg = today_row["value"]["accumchg"].AsDouble;
                //            today_dhg = today_row["value"]["accumdhg"].AsDouble;
                //        }
                //    }

                //    JObject result_row = new JObject();
                //    result_row.Add("accum_dischargingMW", total_dhg);
                //    result_row.Add("accum_chargingMW", total_chg);
                //    result_row.Add("accum_energyMW", total_pv);
                //    result_row.Add("today_accum_energyMW", today_pv);
                //    result_row.Add("today_accum_chargingMW", today_chg);
                //    result_row.Add("today_accum_dischargingMW", today_dhg);



                //    List<double> soc_all = new List<double>();
                //    foreach (RedisKey key in AllSiteKeyByBsc)
                //    {
                //        double dValue;
                //        if(_db.HashGet(key, "Soc").TryParse(out dValue) && dValue != 0)
                //        {
                //            soc_all.Add(dValue);
                //        }
                //    }

                //    result_row.Add("soc_average", soc_all.Average());
                //    return Ok(result_row);

                //}

                //private void UpdateSiteKeyByBsc()
                //{
                //    if (AllSiteKeyByBsc.Count == 0)
                //    {
                //        //            KeyList.Clear();
                //        foreach (EndPoint endPoint in _redisConn.GetEndPoints())
                //        {
                //            string pattern = "site_*_BSC";
                //            IServer server = _redisConn.GetServer(endPoint);
                //            RedisKey[] Keys = server.Keys(pattern: pattern).ToArray();
                //            AllSiteKeyByBsc.AddRange(Keys);
                //        }
                //    }
                //}

                //[HttpGet("DailyAccumPowerBySite")]
                //public async Task<IActionResult> DailyAccumPowerBySite(DateTime date, int siteid)
                //{
                //    var cols = mapreduce_db.GetCollection<BsonDocument>("StatisticsBySite");
                //    var builder = Builders<BsonDocument>.Filter;
                //    var filter = builder.Eq("_id.siteid", siteid) & builder.Eq("_id.timestamp", date.ToString("yyyy-MM-dd"));
                //    IAsyncCursor<BsonDocument> cursor = await cols.FindAsync(filter);
                //    var result = await cursor.ToListAsync();
                //    if (result.Count > 0)
                //    {
                //        JObject jObject = new JObject();

                //        double accumEnergrykW = 0;
                //        double accum_chargingkW = 0;
                //        double accum_dischargingkW = 0;
                //        BsonValue doc = result.FirstOrDefault()["value"];
                //        accumEnergrykW = doc["accum_energykW"].AsDouble;
                //        accum_chargingkW = doc["accum_chargingkW"].AsDouble;
                //        accum_dischargingkW = doc["accum_dischargingkW"].AsDouble;
                //        jObject.Add("siteid", siteid);
                //        jObject.Add("request_date", date);
                //        jObject.Add("accum_energykW", accumEnergrykW);
                //        jObject.Add("accum_chargingkW", accum_chargingkW);
                //        jObject.Add("accum_dischargingkW", accum_dischargingkW);
                //        jObject.Add("last_update_timestmp", doc["timestamp"].AsString);
                //        return Ok(jObject);
                //    }
                //    else
                //    {
                //        return BadRequest();
                //    }
                //    //////List<ServiceModel> result_models = new List<ServiceModel>();
                //    ////await cursor.ForEachAsync(db =>
                //    ////{
                //    ////    JObject obj = JObject.FromObject(db);
                //    ////    jArray.Add(obj);
                //    ////});
                //    //return Ok(result);
                //}

                //[HttpGet("TotalAccumPowerBySite")]
                //public async Task<IActionResult> TotalAccumPowerBySite(int siteid)
                //{
                //    try
                //    {
                //        Console.WriteLine("CALL TOTALACCUMPOWERBYSITE");
                //        var cols = mapreduce_db.GetCollection<BsonDocument>("StatisticsBySite");
                //        var builder = Builders<BsonDocument>.Filter;
                //        var filter = builder.Eq("_id.siteid", siteid);
                //        IAsyncCursor<BsonDocument> cursor = await cols.FindAsync(filter);
                //        JObject jObject = new JObject();

                //        double accum_energykW = 0;
                //        double accum_chargingkW = 0;
                //        double accum_dischargingkW = 0;
                //        await cursor.ForEachAsync(db =>
                //        {
                //            accum_energykW += db["value"]["accum_energykW"].AsDouble;
                //            accum_chargingkW += db["value"]["accum_chargingkW"].AsDouble;
                //            accum_dischargingkW += db["value"]["accum_dischargingkW"].AsDouble;
                //        });

                //        jObject.Add("siteid", siteid);
                //        jObject.Add("accum_energykW", accum_energykW);
                //        jObject.Add("accum_chargingkW", accum_chargingkW);
                //        jObject.Add("accum_dischargingkW", accum_dischargingkW);
                //        return Ok(jObject);
                //    }
                //    catch(Exception ex)
                //    {
                //        logger.LogError(ex, "ERROR!! Call: TotalAccumPowerBySite / SiteId:" + siteid);
                //        return BadRequest(ex);
                //    }
                //    //////List<ServiceModel> result_models = new List<ServiceModel>();
                //    ////await cursor.ForEachAsync(db =>
                //    ////{
                //    ////    JObject obj = JObject.FromObject(db);
                //    ////    jArray.Add(obj);
                //    ////});
                //    //return Ok(result);
                //}
            }
}