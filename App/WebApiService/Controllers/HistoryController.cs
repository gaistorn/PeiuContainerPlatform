using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NHibernate.Criterion;
using PEIU.Events.Alarm;
using PEIU.Models;
using PEIU.Models.Database;
using PEIU.Models.IdentityModel;
using PeiuPlatform.App;

namespace WebApiService.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        AccountEF _accountContext;
        private readonly GridDataContext _peiuGridDataContext;
        IClaimServiceFactory claimsManager;
        readonly ILogger<HistoryController> logger;
        public HistoryController(ILogger<HistoryController> logger,
            GridDataContext peiuGridDataContext, IClaimServiceFactory claimsManager,
            AccountEF accountContext)
        {
            _accountContext = accountContext;
            _peiuGridDataContext = peiuGridDataContext;
            this.claimsManager = claimsManager;
            this.logger = logger;
        }

        [HttpGet, Route("getdailyhistorybysiteid")]
        public async Task<IActionResult> GetDailyHistoryBySiteId(DateTime date, int siteId)
        {
            try
            {
                if (ControlHelper.ValidateSiteId(_accountContext, claimsManager, HttpContext, siteId) == false)
                    return Unauthorized();
                return Ok(await GetDailyHistory(date, new int[] { siteId }));
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }

        }

        [HttpGet, Route("getdailyhistorybyaggid")]
        public async Task<IActionResult> GetDailyHistoryByAggId(DateTime date, string aggid)
        {
            try
            {
                IEnumerable<int> siteIds = ControlHelper.GetAvaliableSiteIdsByAggGroupId(_accountContext, claimsManager, HttpContext, aggid);
                return Ok(await GetDailyHistory(date, siteIds));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
        }

        [HttpGet, Route("getdailyhistorybyrcc")]
        public async Task<IActionResult> GetDailyHistoryByRcc(DateTime date, int rcc)
        {
            try
            {
                IEnumerable<int> siteIds = ControlHelper.GetAvaliableSiteIdsByRcc(_accountContext, claimsManager, HttpContext, rcc);
                return Ok(await GetDailyHistory(date, siteIds));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
        }

        [HttpGet, Route("getmonthlyhistorybysiteid")]
        public async Task<IActionResult> GetMonthlyHistoryBySiteId(int year, int month, int siteId)
        {
            try
            {
                if (ControlHelper.ValidateSiteId(_accountContext, claimsManager, HttpContext, siteId) == false)
                    return Unauthorized();
                return Ok(await GetMonthlyHistory(year, month, new int[] { siteId }));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }

        }

        [HttpGet, Route("getmonthlyhistorybyaggid")]
        public async Task<IActionResult> GetMonthlyHistoryByAggId(int year, int month, string aggid)
        {
            try
            {
                IEnumerable<int> siteIds = ControlHelper.GetAvaliableSiteIdsByAggGroupId(_accountContext, claimsManager, HttpContext, aggid);
                return Ok(await GetMonthlyHistory(year, month, siteIds));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
        }

        [HttpGet, Route("getmonthlyhistorybyrcc")]
        public async Task<IActionResult> GetMonthlyHistoryByRcc(int year, int month, int rcc)
        {
            try
            {
                IEnumerable<int> siteIds = ControlHelper.GetAvaliableSiteIdsByRcc(_accountContext, claimsManager, HttpContext, rcc);
                return Ok(await GetMonthlyHistory(year, month, siteIds));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
        }

        private async Task<IActionResult> GetMonthlyHistory(int Year, int Month, IEnumerable<int> siteId)
        {
            DateTime startDate = new DateTime(Year, Month, 1);
            DateTime endDate = startDate.AddMonths(1);
            ICriterion dateFilter = Restrictions.Ge("Timestamp", startDate) && Restrictions.Lt("Timestamp", endDate);
            using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
            {
                var pcs_result = await session.CreateCriteria<DailyStatistic>()
                    .Add(Restrictions.InG<int>("SiteId", siteId))
                    .Add(dateFilter)
                    .ListAsync<DailyStatistic>();

                var pv_result = await session.CreateCriteria<DailyStatisticsPv>()
                    .Add(Restrictions.InG<int>("SiteId", siteId))
                    .Add(dateFilter)
                    .ListAsync<DailyStatisticsPv>();

                var bms_result = await session.CreateCriteria<DailyStatisticsBms>()
                    .Add(Restrictions.InG<int>("SiteId", siteId)).Add(dateFilter)
                    .ListAsync<DailyStatisticsBms>();

                var revenue_result = await session.CreateCriteria<Hourlyrevenue>()
                    .Add(Restrictions.InG<int>("SiteId", siteId))
                    .Add(dateFilter)
                    .ListAsync<Hourlyrevenue>();

                var event_result = await session.CreateCriteria<VwEventRecord>()
                    .Add(Restrictions.InG<int>("SiteId", siteId))
                    .Add(Restrictions.Ge("CreateDT", startDate) && Restrictions.Lt("CreateDT", endDate))
                    .ListAsync<VwEventRecord>();

                var evt_list = event_result.ToAsyncEnumerable();
                var pcs_list = pcs_result.ToAsyncEnumerable();
                var pv_list = pv_result.ToAsyncEnumerable();
                var rev_list = revenue_result.ToAsyncEnumerable();
                var bms_list = bms_result.ToAsyncEnumerable();

                List<Object> result = new List<object>();

                DateTime dt = startDate;
                while (dt < endDate)
                {
                    double money = await rev_list.Where(x => x.Timestamp.Date == dt).Sum(x => x.Money);
                    result.Add(
                    new
                    {
                        Timestamp = dt,
                        SiteId = siteId,
                        SumOfCharging = await pcs_list.Where(x => x.Timestamp.Value == dt).Sum(x => x.Charging),
                        SumOfDischarging = await pcs_list.Where(x => x.Timestamp.Value == dt).Sum(x => x.Discharging),
                        SumOfPvPower = await pv_list.Where(x => x.Timestamp.Date == dt).Sum(x => x.Accumpvpower),
                        AvgOfSoc = await bms_list.Where(x => x.Timestamp.Value == dt).Average(x => x.Soc),
                        AvgOfSoh = await bms_list.Where(x => x.Timestamp.Value == dt).Average(x => x.Soh),
                        Revenue = money,
                        CountOfEvent = await evt_list.Where(x => x.CreateDT.Date == dt).Count(),
                        OperationRate = 100,
                    });
                    dt = dt.AddDays(1);
                }

                return Ok(result);
                //var pcs_group = pcs_result.GroupBy(x => x.Timestamp.Hour, v => v);
                //var bms_group = bms_result.GroupBy(x => x.Timestamp.Hour, v => v);
                //var pv_rgroup = pv_result.GroupBy(x => x.Timestamp.Hour, v => v);
                //var rev_group = revenue_result.GroupBy(x => x.Timestamp.Hour, v => v);
                //var evt_group = event_result.GroupBy(x => x.CreateDT.Hour, v => v);

            }
        }

        private async Task<IActionResult> GetDailyHistory(DateTime date, IEnumerable<int> siteId)
        {
            using (var session = _peiuGridDataContext.SessionFactory.OpenSession())
            {
                var pcs_result = await session.CreateCriteria<Hourlystatistic>()
                    .Add(Restrictions.InG<int>("SiteId", siteId))
                    .Add(Restrictions.Where<Hourlystatistic>(x => x.Timestamp.Date == date.Date))
                    .ListAsync<Hourlystatistic>();

                var bms_result = await session.CreateCriteria<HourlystatisticsBm>()
                    .Add(Restrictions.InG<int>("SiteId", siteId))
                    .Add(Restrictions.Where<HourlystatisticsBm>(x => x.Timestamp.Date == date.Date))
                    .ListAsync<HourlystatisticsBm>();

                var pv_result = await session.CreateCriteria<HourlystatisticsPv>()
                    .Add(Restrictions.InG<int>("SiteId", siteId))
                    .Add(Restrictions.Where<HourlystatisticsPv>(x => x.Timestamp.Date == date.Date))
                    .ListAsync<HourlystatisticsPv>();

                var revenue_result = await session.CreateCriteria<Hourlyrevenue>()
                    .Add(Restrictions.InG<int>("SiteId", siteId))
                    .Add(Restrictions.Where<Hourlyrevenue>(x => x.Timestamp.Date == date.Date))
                    .ListAsync<Hourlyrevenue>();

                var event_result = await session.CreateCriteria<VwEventRecord>()
                    .Add(Restrictions.InG<int>("SiteId", siteId))
                    .Add(Restrictions.Where<VwEventRecord>(x => x.CreateDT.Date == date.Date))
                    .ListAsync<VwEventRecord>();

                var evt_list = event_result.ToAsyncEnumerable();
                var pcs_list = pcs_result.ToAsyncEnumerable();
                var pv_list = pv_result.ToAsyncEnumerable();
                var rev_list = revenue_result.ToAsyncEnumerable();
                var bms_list = bms_result.ToAsyncEnumerable();

                List<Object> result = new List<object>();

                for (int hour = 0; hour <= 23; hour++)
                {
                    double money = await rev_list.Where(x => x.Hour == hour).Sum(x => x.Money);

                    Hourlyrevenue rev = await rev_list.FirstOrDefault(x => x.Hour == hour);
                    //await evt_list.Where(x=>x.CreateDT)
                    result.Add(
                    new
                    {
                        Timestamp = date.Date.AddHours(hour),
                        SiteId = siteId,
                        SumOfCharging = await pcs_list.Where(x => x.Timestamp.Hour == hour).Sum(x => x.Charging),
                        SumOfDischarging = await pcs_list.Where(x => x.Timestamp.Hour == hour).Sum(x => x.Discharging),
                        AvgOfSoc = await bms_list.Where(x => x.Timestamp.Hour == hour).Average(x => x.Soc),
                        AvgOfSoh = await bms_list.Where(x => x.Timestamp.Hour == hour).Average(x => x.Soh),
                        SumOfPvPower = await pv_list.Where(x => x.Timestamp.Hour == hour).Sum(x => x.Accumpvpower),
                        Revenue = money,
                        CountOfEvent = await evt_list.Where(x => x.CreateDT.Hour == hour).Count(),
                        OperationRate = 100,
                    }); ;
                }


                return Ok(result);
                //var pcs_group = pcs_result.GroupBy(x => x.Timestamp.Hour, v => v);
                //var bms_group = bms_result.GroupBy(x => x.Timestamp.Hour, v => v);
                //var pv_rgroup = pv_result.GroupBy(x => x.Timestamp.Hour, v => v);
                //var rev_group = revenue_result.GroupBy(x => x.Timestamp.Hour, v => v);
                //var evt_group = event_result.GroupBy(x => x.CreateDT.Hour, v => v);

            }
        }
    }
}