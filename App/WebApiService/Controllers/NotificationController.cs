using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NHibernate;
using NHibernate.Criterion;
using PEIU.Events.Alarm;
//using PEIU.Models;
using PEIU.Models.Database;
using PEIU.Models.IdentityModel;
using PeiuPlatform.App;
using PES.Toolkit;
using StackExchange.Redis;

namespace WebApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        readonly PeiuGridDataContext peiuGridDataContext;
        readonly IDatabaseAsync db;
        readonly AccountEF _accountContext;
        readonly ILogger<NotificationController> logger;
        private readonly IClaimServiceFactory _claimsManager;
        public NotificationController(PeiuGridDataContext peiuGridDataContext, IRedisConnectionFactory redis, 
            AccountEF accountContext, ILogger<NotificationController> logger,  IClaimServiceFactory claimsManager, 
        IClaimServiceFactory claimServiceFactory)
        {
            this.peiuGridDataContext = peiuGridDataContext;
            db = redis.Connection().GetDatabase(1);
            this._accountContext = accountContext;
            this.logger = logger;
            this._claimsManager = claimsManager;
        }

        private ICriteria ApplyEventFilter(ICriteria criteria, int siteId,  bool OnlyGetCompleteEvent)
        {
            
            ICriteria newcriteria = criteria;
            if (siteId != -1)
                newcriteria = newcriteria.Add(Restrictions.Eq("SiteId", siteId));
            else
            {
                var avaliableSiteIds = ControlHelper.GetAvaliableSiteIds(_accountContext, _claimsManager, HttpContext);
                newcriteria = newcriteria.Add(Restrictions.InG<int>("SiteId", avaliableSiteIds));
            }

            if (OnlyGetCompleteEvent)
                newcriteria = newcriteria.Add(Restrictions.IsNotNull("RecoveryDT")).Add(Restrictions.IsNotNull("AckDT"));
            else
            {
                newcriteria = newcriteria.Add(Restrictions.IsNull("RecoveryDT") || Restrictions.IsNull("AckDT"));
            }
            return newcriteria;
        }


        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getactiveeventlist")]
        public async Task<IActionResult> GetActiveEventList(int siteId = -1, bool onlycompletedevent = false)
        {
            using (var session = peiuGridDataContext.SessionFactory.OpenStatelessSession())
            {
                var map_row = await ApplyEventFilter(session.CreateCriteria<VwEventRecord>(), siteId, onlycompletedevent).ListAsync<VwEventRecord>();
                return Ok(map_row);
            }
        }

        [Authorize(Policy = UserPolicyTypes.OnlySupervisor)]
        [HttpGet("geteventhistorybyagg")]
        public async Task<IActionResult> geteventhistorybyagg(DateTime startdate, DateTime enddate, int PageNo, int ShowRowCount, string aggId)
        {
            try
            {
                IEnumerable<int> AvaliableSiteIds = null;
                AvaliableSiteIds = ControlHelper.GetAvaliableSiteIdsByAggGroupId(_accountContext, _claimsManager, HttpContext, aggId);

                if (AvaliableSiteIds.Count() == 0)
                {
                    return BadRequest();
                }
                //rcc
                using (var session = peiuGridDataContext.SessionFactory.OpenSession())
                {
                    var query_results = await session.CreateCriteria<VwEventRecord>()
                        .Add(Restrictions.IsNotNull("AckDT") && Restrictions.InG<int>("SiteId", AvaliableSiteIds) && Restrictions.Between("CreateDT", startdate, enddate))
                        .ListAsync<VwEventRecord>();

                    return Ok(query_results.Skip((PageNo - 1) * ShowRowCount).Take(ShowRowCount).OrderByDescending(x => x.CreateDT));

                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
            //db.getCollection('daegun_meter').find({ sSiteId: 148}).skip((4 - 1) * 10).limit(10)
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet("geteventhistorybysiteid")]
        public async Task<IActionResult> geteventhistorybysiteid(DateTime startdate, DateTime enddate, int PageNo, int ShowRowCount, int siteid = -1)
        {
            try
            {
                IEnumerable<int> AvaliableSiteIds = null;
                if (siteid == -1)
                {
                    AvaliableSiteIds = ControlHelper.GetAvaliableSiteIds(_accountContext, _claimsManager, HttpContext);
                }
                else
                    AvaliableSiteIds = new int[] { siteid };

                if (AvaliableSiteIds.Count() == 0)
                {
                    return BadRequest();
                }
                //rcc
                using (var session = peiuGridDataContext.SessionFactory.OpenSession())
                {
                    var query_results = await session.CreateCriteria<VwEventRecord>()
                        .Add(Restrictions.IsNotNull("AckDT") && Restrictions.InG<int>("SiteId", AvaliableSiteIds) && Restrictions.Between("CreateDT", startdate, enddate))
                        .ListAsync<VwEventRecord>();

                    return Ok(query_results.Skip((PageNo - 1) * ShowRowCount).Take(ShowRowCount).OrderByDescending(x => x.CreateDT));

                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
            //db.getCollection('daegun_meter').find({ sSiteId: 148}).skip((4 - 1) * 10).limit(10)
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet("geteventhistorybyrcc")]
        public async Task<IActionResult> geteventhistorybyrcc(DateTime startdate, DateTime enddate, int PageNo, int ShowRowCount, int rcc = -1)
        {
            try
            {
                IEnumerable<int> AvaliableSiteIds = null;
                if (rcc == -1)
                {
                    AvaliableSiteIds = ControlHelper.GetAvaliableSiteIds(_accountContext, _claimsManager, HttpContext);
                }
                else
                    AvaliableSiteIds = ControlHelper.GetAvaliableSiteIdsByRcc(_accountContext, _claimsManager, HttpContext, rcc);
                if (AvaliableSiteIds.Count() == 0)
                {
                    return Ok();
                }
                //rcc
                using (var session = peiuGridDataContext.SessionFactory.OpenSession())
                {
                    var query_results = await session.CreateCriteria<VwEventRecord>()
                        .Add(Restrictions.IsNotNull("AckDT") && Restrictions.InG<int>("SiteId", AvaliableSiteIds) && Restrictions.Between("CreateDT", startdate, enddate))
                        .ListAsync<VwEventRecord>();

                    return Ok(query_results.Skip((PageNo - 1) * ShowRowCount).Take(ShowRowCount).OrderByDescending(x => x.CreateDT));

                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
            //db.getCollection('daegun_meter').find({ sSiteId: 148}).skip((4 - 1) * 10).limit(10)
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet("getactiveevent")]
        public async Task<IActionResult> GetActiveEventList(int PageNo, int ShowRowCount, int rcc = -1)
        {
            try
            {
                IEnumerable<int> AvaliableSiteIds = null;
                if (rcc == -1)
                {
                    AvaliableSiteIds = ControlHelper.GetAvaliableSiteIds(_accountContext, _claimsManager, HttpContext);
                }
                else
                    AvaliableSiteIds = ControlHelper.GetAvaliableSiteIdsByRcc(_accountContext, _claimsManager, HttpContext, rcc);
                if (AvaliableSiteIds.Count() == 0)
                {
                    return Ok();
                }
                //rcc
                using (var session = peiuGridDataContext.SessionFactory.OpenSession())
                {
                    var query_results = await session.CreateCriteria<VwEventRecord>()
                        .Add(Restrictions.IsNull("AckDT") && Restrictions.InG<int>("SiteId", AvaliableSiteIds))
                        .ListAsync<VwEventRecord>();
                    return Ok(query_results.Skip((PageNo - 1) * ShowRowCount).Take(ShowRowCount).OrderByDescending(x => x.CreateDT));

                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
            //db.getCollection('daegun_meter').find({ sSiteId: 148}).skip((4 - 1) * 10).limit(10)
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("setackevent")]
        public async Task<IActionResult> SetAckEvent(int eventId)
        {
            try
            {
                string email = _claimsManager.GetClaimsValue(HttpContext.User, ClaimTypes.Email);
                string userName = _claimsManager.GetClaimsValue(HttpContext.User, ClaimTypes.Name);
                using (var session = peiuGridDataContext.SessionFactory.OpenStatelessSession())
                using (var Transaction = session.BeginTransaction())
                {
                    var map_row = await session.GetAsync<EventRecord>(eventId);
                    if (map_row == null)
                        return BadRequest("존재하지 않는 이벤트 입니다");
                    else if (map_row.AckDT.HasValue)
                        return BadRequest("이미 ACK가 확인된 이벤트입니다");
                    else if (map_row.RecoveryDT.HasValue == false)
                        return BadRequest("아직 복구되지 않은 이벤트입니다");
                    map_row.AckDT = DateTime.Now;
                    map_row.AckUserEmail = email;
                    map_row.AckUserName = userName;
                    await session.UpdateAsync(map_row);
                    await Transaction.CommitAsync();

                }
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getcurrenteventstatus")]
        public async Task<IActionResult> getcurrenteventstatus(int siteId, string deviceName, int index)
        {
            try
            {
                int deviceType = 1;
                if (deviceName.ToUpper() == "PCS")
                    deviceType = 1;
                else if (deviceName.ToUpper() == "BMS")
                    deviceType = 2;
                else if (deviceName.ToUpper() == "PV")
                    deviceType = 3;
                else
                {
                    throw new ArgumentException("존재하지 않는 설비명");
                }
                JArray result = new JArray();
                VwContractorsiteEF site_info = await _accountContext.VwContractorsites.FindAsync(siteId);
                if (site_info == null)
                    return BadRequest();


                using (var session = peiuGridDataContext.SessionFactory.OpenSession())
                {
                    var map_row = await session.CreateCriteria<PEIU.Events.Alarm.ModbusDiMap>()
                        .Add(Restrictions.Eq("FactoryCode", site_info.DeviceGroupCode) && Restrictions.Eq("DeviceType", deviceType))
                        .ListAsync<PEIU.Events.Alarm.ModbusDiMap>();
                    //map_row = await session.CreateCriteria<PEIU.Events.Alarm.ModbusDiMap>().ListAsync<PEIU.Events.Alarm.ModbusDiMap>();
                    foreach (PEIU.Events.Alarm.ModbusDiMap map in map_row)
                    {
                        JObject row = new JObject();
                        row.Add("code", map.EventCode);

                        row.Add("name", map.Name);
                        row.Add("category", map.DeviceType);
                        row.Add("level", map.Level);


                        string redisKey = $"SID{siteId}.DI.{deviceType}.{index}";
                        if (await db.HashExistsAsync(redisKey, map.EventCode))
                        {
                            string status = await db.HashGetAsync(redisKey, map.EventCode);
                            row.Add("status", status);
                        }
                        else
                        {
                            await db.HashSetAsync(redisKey, map.EventCode, bool.FalseString);
                            row.Add("status", bool.FalseString);
                        }

                        result.Add(row);
                    }
                }
                return Ok(result);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

    }
}