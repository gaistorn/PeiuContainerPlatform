using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using PEIU.Models;
using PES.Toolkit;
using StackExchange.Redis;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PeiuPlatform.App.Controllers
{
    [Route("api/device")]
    [Produces("application/json")]
    //[Authorize]
    //[EnableCors(origins: "http://www.peiu.co.kr:3011", headers: "*", methods: "*")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        readonly GridDataContext peiuGridDataContext;
        readonly AccountEF peiuDataContext;
        readonly IDatabase database;

        public DeviceController(GridDataContext context, AccountEF _accountContext, IRedisConnectionFactory redisConnectionFactory)
        {
            peiuGridDataContext = context;
            peiuDataContext = _accountContext;
            database = redisConnectionFactory.Connection().GetDatabase(1);
        }

        [HttpGet("GetDeviceInfo")]
        public async Task<IActionResult> GetDeviceInfo(int SiteId, string DeviceName, string PropertyName)
        {
            string redisKey = $"{SiteId}.{DeviceName}";
            if( await database.KeyExistsAsync(redisKey) == false)
            {
                return BadRequest(string.Format("해당 사이트 아이디: {0} 또는 디바이스 아이디: {1}를 찾을 수 없습니다", SiteId, DeviceName));
            }

            RedisValue rv = await database.HashGetAsync(redisKey, PropertyName);
            return Ok(rv);
        }

        [HttpGet("SetDeviceInfo")]
        public async Task<IActionResult> SetDeviceInfo(int SiteId, string DeviceName, string PropertyName, string Value)
        {
            string redisKey = $"{SiteId}.{DeviceName}";
            if (await database.KeyExistsAsync(redisKey) == false)
            {
                return BadRequest(string.Format("해당 사이트 아이디: {0} 또는 디바이스 아이디: {1}를 찾을 수 없습니다", SiteId, DeviceName));
            }

            await database.HashSetAsync(redisKey, PropertyName, Value);
            return Ok();
        }

        [HttpGet("GetRequestActivePower")]
        public async Task<IActionResult> GetRequestActivePower(int Rcc)
        {
            JArray eventList = new JArray();
            using (var session = peiuGridDataContext.SessionFactory.OpenStatelessSession())
            using (var trans = session.BeginTransaction())
            {
                //var assets = peiuDataContext.AssetLocations.Where(x => x.RCC == Rcc);
                //foreach (AssetLocation assetLocation in assets)
                //{
                //    var list = await session.CreateCriteria<vw_ActiveEvent>().Add(
                //   // Restrictions.Ge("OccurTimestamp", DateTime.Now.AddMinutes(5))).Add(
                //   Restrictions.Eq("SiteId", assetLocation.SiteId)

                //   )
                  
                //   .ListAsync<vw_ActiveEvent>();

                //    foreach (vw_ActiveEvent ev in list)
                //        eventList.Add(JObject.FromObject(ev));
                //}
            }

            return Ok(eventList);
        }

        //[HttpGet("EventAck")]
        //public async Task<IActionResult> EventAck([FromBody] JObject body)
        //{
        //    //body.TryGetValue()
        //    Console.WriteLine($"Params: body");
        //    IEnumerable<string> EventIds = null;
        //    JToken token;
        //    if(body.TryGetValue("EventIds", out token))
        //    {
        //        EventIds = token.Values<string>();
        //    }
        //    using (var session = peiuGridDataContext.SessionFactory.OpenStatelessSession())
        //    using (var trans = session.BeginTransaction())
        //    {
        //        DateTime ackTime = DateTime.Now;

        //        foreach (string eventid in EventIds)
        //        {
        //            DeviceEvent de = session.Get<DeviceEvent>(eventid);
        //            if (de != null && de.IsAck == false)
        //            {
        //                de.AckTimestamp = ackTime;
        //                de.IsAck = true;
        //                await session.UpdateAsync(de);
        //                Console.WriteLine($"eventId: {eventid} ACK Completes");
        //            }
        //        }
        //        await trans.CommitAsync();

        //    }
        //    return Ok();
        //}
    }
}
