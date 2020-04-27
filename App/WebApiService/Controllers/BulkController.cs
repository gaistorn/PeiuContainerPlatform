using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using PEIU.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PeiuPlatform.App.Controllers
{
    [Produces("application/json")]
    //[Authorize]
    //[EnableCors(origins: "http://www.peiu.co.kr:3011", headers: "*", methods: "*")]
    [ApiController]
    [Route("api/[controller]")]
    public class BulkController : ControllerBase
    {
        private readonly GridDataContext peiuGridDataContext;
        private readonly ILogger<BulkController> logger;
        public BulkController(GridDataContext peiuGridDataContext, ILogger<BulkController> logger)
        {
            this.peiuGridDataContext = peiuGridDataContext;
            this.logger = logger;

        }
        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpGet("downloadpvdata")]
        public async Task<IActionResult> DownloadPvData(int year, int month)
        {
            try
            {
                using(IStatelessSession session = peiuGridDataContext.SessionFactory.OpenStatelessSession())
                {
                    DateTime startDt = new DateTime(year, month, 1);
                    DateTime endDt = startDt.AddMonths(1).AddDays(-1);
                    var pv_datas = await session.CreateCriteria<PvData>()
                        .Add(Restrictions.Eq("siteId", (short)6))
                        .Add(Restrictions.Where<PvData>(x => x.timestamp.Year == year && x.timestamp.Month == month))
                        .AddOrder(Order.Asc("timestamp")).ListAsync<PvData>();

                    var groupsData = pv_datas.GroupBy(key => key.siteId, value => value);

                    var weathers = await session.CreateCriteria<Currentweather>()
                        .Add(Restrictions.Where<Currentweather>(x => x.Timestamp.Year == year && x.Timestamp.Month == month))
                        .AddOrder(Order.Asc("Timestamp"))
                        .ListAsync<Currentweather>();

                    var weather_result = weathers.ToArray();
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("siteid,pv_ts,w_ts,year,month,day,hour,gen,temp,humidity,cloud");
                    Currentweather currentweather = null;
                    foreach (IGrouping<short?, PvData> pvData in groupsData)
                    {
                        var timegroupsData = pvData.GroupBy(x => new DateTime(x.timestamp.Year, x.timestamp.Month, x.timestamp.Day, x.timestamp.Hour, 0, 0), value => value);

                        short siteid = pvData.Key.Value;
                        foreach(IGrouping<DateTime, PvData> row in pvData)
                        {
                            DateTime pv_ts = row.Key;
                            Currentweather target_weather = weathers.LastOrDefault(x => x.Timestamp <= pv_ts && x.Timestamp >= pv_ts.AddHours(-1));
                            if (target_weather != null)
                                currentweather = target_weather;
                            else if (target_weather == null && currentweather != null)
                                target_weather = currentweather;

                            sb.AppendLine($"{siteid},{pv_ts}{target_weather.Timestamp}{pv_ts.ToString("yyyy,MM,dd,HH")},{row.Sum(x => x.TotalActivePower) / 3600},{target_weather.Temp},{target_weather.Humidity},{target_weather.Clouds}");
                        }
                    }
                    string str = sb.ToString();
                    byte[] dataBytes = Encoding.UTF8.GetBytes(str);
                    string file = $"PV발전량이력_{year}_{month}.csv";
                    // Response...
                    System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
                    {
                        FileName = file,
                        Inline = false  // false = prompt the user for downloading;  true = browser to try to show the file inline
                    };
                    Response.Headers.Add("Content-Disposition", cd.ToString());
                    Response.Headers.Add("X-Content-Type-Options", "nosniff");
                    return File(dataBytes, "application/octet-stream");

                }
            }catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
        }
    }
}
