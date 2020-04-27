using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PEIU.Models;
using PEIU.Models.Database;
using PEIU.Models.DatabaseModel;
using PEIU.Models.ExchangeModel;
using PeiuPlatform.App.Localization;

namespace PeiuPlatform.App.Controllers
{
    [Route("api/contract")]
    [Produces("application/json")]
    //[Authorize]
    //[EnableCors(origins: "http://www.peiu.co.kr:3011", headers: "*", methods: "*")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly UserManager<UserAccountEF> userManager;
        ILogger<ContractController> logger;
        AccountEF accountContext;
        private readonly LocalizedIdentityErrorDescriber describer;
        public ContractController(IConfiguration configuration, ILoggerFactory loggerFactory, AccountEF _accountContext, UserManager<UserAccountEF> _userManager)
        {
            logger = loggerFactory.CreateLogger<ContractController>();
            accountContext = _accountContext;
            userManager = _userManager;
            describer = userManager.ErrorDescriber as LocalizedIdentityErrorDescriber;
        }

       

        [HttpGet("getassetbysiteid")]
        public async Task<IActionResult> GetAssetBySiteId(int siteId)
        {
            IEnumerable<ContractorAssetEF> result = accountContext.ContractorAssets.Where(x => x.SiteId == siteId);
            JArray return_value = new JArray(result);
            //foreach (AssetLocation loc in result)
            //{
            //    JObject obj = JObject.FromObject(loc);
            //    return_value.Add(obj);
            //}
            return Ok(return_value);
        }

        

        //[HttpGet("getreservedregisters")]
        //public async Task<IActionResult> GetReservedRegisters()
        //{
        //    JArray array = new JArray();
        //    foreach (var account in accountContext.ReservedAssetLocations)
        //    {
        //        JObject row = JObject.FromObject(account);
        //        array.Add(row);
        //    }
        //    return Ok(array);
        //}

        //[HttpGet("getcontractorlist")]
        //public async Task<IActionResult> GetContractorList()
        //{
        //    JArray array = new JArray();
        //    foreach(AccountModel account in accountContext.Users)
        //    {
        //        JObject row = JObject.FromObject(account);
        //        array.Add(row);
        //    }
        //    return Ok(array);
        //}

        //// GET: api/Contract
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET: api/Contract/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/Contract
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Contract/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
