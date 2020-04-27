using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PES.Toolkit;
using StackExchange.Redis;

namespace PeiuPlatform.App.Controllers
{
    [Route("api/pms")]
    [Produces("application/json")]
    [ApiController]
    public class PMSController : ControllerBase
    {
        static ILogger logger;
        static IDatabase _db;

        public PMSController(ILoggerFactory loggerFactory, IRedisConnectionFactory redisConnectionFactory)
        {
            logger = loggerFactory.CreateLogger<PMSController>();
            _db = redisConnectionFactory.Connection().GetDatabase();

        }

        //[HttpGet("stat")]
        //public async Task<IActionResult> GetStatRcc()
        //{
        //    JArray stats = new JArray();
        //    for(int i=1;i<=14;i++)
        //    {
        //        JObject row = new JObject();
        //        row.Add("requst_id", $"ESS.statistics.rsc.{i}");
        //        stats.Add(row);
        //        if (RccBySiteMap.ContainsKey(i) == false)
        //        {
        //            continue;
        //        }

        //        Dictionary<int, List<JObject>> datasBySiteId = new Dictionary<int, List<JObject>>()
        //        {
        //            {0, new List<JObject>() }, // PCS
        //            {1, new List<JObject>() }, // BSC
        //            {2, new List<JObject>() }, // PV
        //             {3, new List<JObject>() }
        //        };
        //        foreach(int siteId in RccBySiteMap[i])
        //        {
        //            JObject pcs_data = await GetJObject(siteId, "PCS");
        //            JObject ess_data = await GetJObject(siteId, "ESSMETER");
        //            JObject bsc_data = await GetJObject(siteId, "BSC");
        //            JObject pv_data = await GetJObject(siteId, "PVMETER");
        //            datasBySiteId[0].Add(pcs_data);
        //            datasBySiteId[1].Add(bsc_data);
        //            datasBySiteId[2].Add(pv_data);
        //            datasBySiteId[3].Add(ess_data);
        //        }

        //        float soc = datasBySiteId[1].Select(x => x.Value<float>("Soc")).Average();
        //        double pv_active = datasBySiteId[2].Select(x => x.Value<double>("TotalActivePower")).Average();
        //        int actPwr = datasBySiteId[0].Select(x => x.Value<int>("ActivePower")).Sum();
        //        row.Add("soc_average", soc);
        //        row.Add("pv_activePower", pv_active);
        //        row.Add("pcs_activePower", actPwr);
        //    }

        //    return Ok(stats);
        //}


        // GET: api/PMS
        [HttpGet("pcs")]
        public async Task<IActionResult> getpcsdata(int siteid)
        {
            JObject getModel = await GetJObject(siteid, "PCS");
            if (getModel == null)
            {
                return Ok(StatusCodes.Status400BadRequest);
            }
            else
                return Ok(getModel);
        }

       

        // GET: api/PMS
        [HttpGet("bsc")]
        public async Task<IActionResult> getbscdata(int siteid)
        {
            JObject getModel = await GetJObject(siteid, "BSC");
            if (getModel == null)
            {
                return Ok(StatusCodes.Status400BadRequest);
            }
            else
                return Ok(getModel);
        }

        // GET: api/PMS
        [HttpGet("ess")]
        public async Task<IActionResult> getessdata(int siteid)
        {
            JObject getModel = await GetJObject(siteid, "ESSMETER");
            if (getModel == null)
            {
                return Ok(StatusCodes.Status400BadRequest);
            }
            else
                return Ok(getModel);
        }

        // GET: api/PMS
        [HttpGet("pv")]
        public async Task<IActionResult> getpvdata(int siteid)
        {
            JObject getModel = await GetJObject(siteid, "PVMETER");
            if (getModel == null)
            {
                return Ok(StatusCodes.Status400BadRequest);
            }
            else
                return Ok(getModel);
        }

        private Task<JObject> GetJObject(int siteid, string deviceid)
        {
            return Task<JObject>.Run<JObject>(async () =>
            {
                JObject obj = new JObject();
                try
                {
                    string key = $"site_{siteid}_{deviceid}";
                    bool isExist = await _db.KeyExistsAsync(key);
                    if(isExist == false)
                    {
                        return null;
                    }
                    HashEntry[] entries = await _db.HashGetAllAsync(key);

                    obj.Add("request_id", key);
                    foreach (HashEntry entry in entries)
                    {
                        double dValue;
                        long lValue;
                        int iValue;

                        if (entry.Value.TryParse(out iValue))
                            obj.Add(entry.Name, iValue);
                        else if (entry.Value.TryParse(out lValue))
                            obj.Add(entry.Name, lValue);
                        else if (entry.Value.TryParse(out dValue))
                            obj.Add(entry.Name, dValue);
                        else
                            obj.Add(entry.Name, entry.Value.ToString());
                    }

                }
                catch (Exception ex)
                {
                    obj.Add("Error type", ex.GetType().ToString());
                    obj.Add("Error Message", ex.Message);
                }
                return obj;
            });

        }
    }
}
