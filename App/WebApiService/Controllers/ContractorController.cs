using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using PEIU.Models;
using PEIU.Models.Database;
using PEIU.Models.ExchangeModel;
using PEIU.Models.IdentityModel;
using PeiuPlatform.App.Localization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PeiuPlatform.App.Controllers
{
    [Route("api/[controller]")]
    public class ContractorController : ControllerBase
    {

        private readonly UserManager<UserAccountEF> _userManager;
        AccountEF _accountContext;
        private readonly IStringLocalizer<LocalizedIdentityErrorDescriber> _localizer;
        private readonly SignInManager<UserAccountEF> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<Role> roleManager;
        private readonly IHTMLGenerator htmlGenerator;
        private readonly IClaimServiceFactory _claimsManager;

        public ContractorController(UserManager<UserAccountEF> userManager,
            SignInManager<UserAccountEF> signInManager, RoleManager<Role> _roleManager,
            IEmailSender emailSender, IHTMLGenerator _htmlGenerator, IClaimServiceFactory claimsManager,
            IStringLocalizer<LocalizedIdentityErrorDescriber> localizer, 
            AccountEF accountContext)
        {
            _userManager = userManager;
            _accountContext = accountContext;
            _signInManager = signInManager;
            _localizer = localizer;
            _emailSender = emailSender;
            htmlGenerator = _htmlGenerator;
            roleManager = _roleManager;
            _claimsManager = claimsManager;
        }

        private async Task<string> GetAggregatorGroupId(ClaimsPrincipal claimsPrincipal)
        {
            var agg_group_id = claimsPrincipal.FindFirstValue(UserClaimTypes.AggregatorGroupIdentifier);
            return agg_group_id;
        }

       
        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet, Route("getcontractors")]
        public async Task<IActionResult> GetContractors(string aggregatorgroupid = null)
        {
            try
            {
                IEnumerable<VwContractoruserEF> source = null;

                if (HttpContext.User.IsInRole(UserRoleTypes.Supervisor))
                {
                    if(aggregatorgroupid != null)
                    {
                        source = _accountContext.VwContractorusers.Where(x => x.AggGroupId == aggregatorgroupid);
                    }
                    else
                        source = _accountContext.VwContractorusers;
                }
                else if(HttpContext.User.IsInRole(UserRoleTypes.Contractor))
                {   
                    string userId = _claimsManager.GetClaimsValue(HttpContext.User, ClaimTypes.NameIdentifier);
                    source = _accountContext.VwContractorusers.Where(x => x.UserId == userId);
                }
                else if(HttpContext.User.IsInRole(UserRoleTypes.Aggregator))
                {
                    string groupId = _claimsManager.GetClaimsValue(HttpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                    source = _accountContext.VwContractorusers.Where(x => x.AggGroupId == groupId);
                }
               // var source = aggregatorgroupid == null ? _accountContext.VwContractorusers : _accountContext.VwContractorusers.Where(x => x.AggGroupId == aggregatorgroupid);
                //JArray jr = new JArray(_accountContext.VwContractorusers);
                // foreach (var contractor in source)
                //{
                //    JObject obj = JObject.FromObject(contractor);
                //    result.Add(obj);
                //    //var user = await _userManager.FindByIdAsync(contractor.UserId);
                //    //if (user != null)
                //    //{
                        
                //    //    var json_usr = ModelConverter.ConvertToJsonJObject<UserAccount>(user, "password", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", 
                //    //        "TwoFactorEnabled", "NormalizedUserName", "PhoneNumberConfirmed", "TwoFactorEnabled");
                //    //    json_usr.Add("ContractStatus", (int)contractor.ContractStatus);
                //    //    json_usr.Add("AggregatorId", contractor.AggGroupId);
                //    //    result.Add(json_usr);
                //    //}
                //}
                return Ok(source);
            }
            catch(Exception ex)
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = UserRoleTypes.Supervisor)]
        [HttpGet("connectsitetocustomer")]
        public async Task ConnectSiteToCustomer(string userid, int siteid)
        {
            var site = await _accountContext.ContractorSites.FindAsync(siteid);
            if(site == null)
            {
                BadRequest("없는 사이트");
            }

            site.ContractUserId = userid;
            await _accountContext.SaveChangesAsync();
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet("getcontractorsites")]
        public async Task<IActionResult> GetContractorSites(string aggregatorgroupid = null, string contractorid = null)
        {
            List<VwContractorsiteEF> source = null;
            try
            {
                if (HttpContext.User.IsInRole(UserRoleTypes.Supervisor))
                {
                    if(string.IsNullOrEmpty(aggregatorgroupid) == false)
                    {
                        source = new List<VwContractorsiteEF>();
                        foreach (VwContractoruserEF user in _accountContext.VwContractorusers.Where(x => x.AggGroupId == aggregatorgroupid))
                        {
                            source.AddRange(_accountContext.VwContractorsites.Where(x => x.UserId != null && x.UserId == user.UserId));
                        }
                    }
                    else
                    {
                        source = _accountContext.VwContractorsites.Where(x=>x.UserId != null).ToList();
                    }
                }
                else if (HttpContext.User.IsInRole(UserRoleTypes.Contractor))
                {
                    string userId = _claimsManager.GetClaimsValue(HttpContext.User, ClaimTypes.NameIdentifier);
                    source = _accountContext.VwContractorsites.Where(x => x.UserId != null && x.UserId == userId).ToList();
                }
                else if (HttpContext.User.IsInRole(UserRoleTypes.Aggregator))
                {
                    string groupId = _claimsManager.GetClaimsValue(HttpContext.User, UserClaimTypes.AggregatorGroupIdentifier);

                    source = new List<VwContractorsiteEF>();
                    if (string.IsNullOrEmpty(contractorid) == false)
                        source.AddRange(_accountContext.VwContractorsites.Where(x => x.UserId != null && x.UserId == contractorid && x.AggGroupId == groupId));
                    else
                    {
                        foreach (VwContractoruserEF user in _accountContext.VwContractorusers.Where(x => x.AggGroupId == groupId))
                        {
                            source.AddRange(_accountContext.VwContractorsites.Where(x => x.UserId != null && x.UserId == user.UserId));
                        }
                    }
                    
                }
                else
                    return BadRequest();


                //JArray array = new JArray();
                //foreach (VwContractorsite account in source)
                //{
                //    JObject row = JObject.FromObject(account);
                //    var pcs_list = _accountContext.ContractorAssets.Where(x => x.AssetType == AssetTypeCodes.PCS && x.SiteId == account.SiteId).ToArray();
                //    var bms_list = _accountContext.ContractorAssets.Where(x => x.AssetType == AssetTypeCodes.BMS && x.SiteId == account.SiteId).ToArray();
                //    var pv_list = _accountContext.ContractorAssets.Where(x => x.AssetType == AssetTypeCodes.PV && x.SiteId == account.SiteId).ToArray();
                //    double pcsCapacity = pcs_list.Sum(x => x.CapacityKW);
                //    double bmsCapacity = bms_list.Sum(x => x.CapacityKW);
                //    double pvCapacity = pv_list.Sum(x => x.CapacityKW);
                //    row.Add("TotalPcsCapacity", pcsCapacity);
                //    row.Add("TotalBmsCapacity", bmsCapacity);
                //    row.Add("TotalPvCapacity", pvCapacity);
                //    row.Add("PcsCount", pcs_list.Count());
                //    row.Add("BmsCount", bms_list.Count());
                //    row.Add("PvCount", pv_list.Count());
                //    array.Add(row);

                //}
                return Ok(source);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
                
        }

        [Authorize(Policy = UserPolicyTypes.AllUserPolicy)]
        [HttpGet("getallcontractorsites")]
        public async Task<IActionResult> GetAllContractorSites(string aggregatorgroupid = null, bool onlyunspecifieduser = false)
        {
            List<VwContractorsiteEF> source = null;
            try
            {
                if (HttpContext.User.IsInRole(UserRoleTypes.Supervisor))
                {
                    if (aggregatorgroupid != null)
                    {
                        source = new List<VwContractorsiteEF>();
                        foreach (VwContractoruserEF user in _accountContext.VwContractorusers.Where(x => x.AggGroupId == aggregatorgroupid))
                        {
                            source.AddRange(_accountContext.VwContractorsites.Where(x => x.UserId == user.UserId));
                        }
                    }
                    else
                    {
                        if(onlyunspecifieduser)
                            source = _accountContext.VwContractorsites.Where(x=>x.UserId == null).ToList();
                        else
                            source = _accountContext.VwContractorsites.ToList();
                    }
                }
                else if (HttpContext.User.IsInRole(UserRoleTypes.Contractor))
                {
                    string userId = _claimsManager.GetClaimsValue(HttpContext.User, ClaimTypes.NameIdentifier);
                    source = _accountContext.VwContractorsites.Where(x => x.UserId == userId).ToList();
                }
                else if (HttpContext.User.IsInRole(UserRoleTypes.Aggregator))
                {
                    string groupId = _claimsManager.GetClaimsValue(HttpContext.User, UserClaimTypes.AggregatorGroupIdentifier);

                    source = new List<VwContractorsiteEF>();
                    foreach (VwContractoruserEF user in _accountContext.VwContractorusers.Where(x => x.AggGroupId == groupId))
                    {
                        source.AddRange(_accountContext.VwContractorsites.Where(x => x.UserId == user.UserId));
                    }
                }
                else
                    return BadRequest();


                //JArray array = new JArray();
                //foreach (VwContractorsite account in source)
                //{
                //    JObject row = JObject.FromObject(account);
                //    var pcs_list = _accountContext.ContractorAssets.Where(x => x.AssetType == AssetTypeCodes.PCS && x.SiteId == account.SiteId).ToArray();
                //    var bms_list = _accountContext.ContractorAssets.Where(x => x.AssetType == AssetTypeCodes.BMS && x.SiteId == account.SiteId).ToArray();
                //    var pv_list = _accountContext.ContractorAssets.Where(x => x.AssetType == AssetTypeCodes.PV && x.SiteId == account.SiteId).ToArray();
                //    double pcsCapacity = pcs_list.Sum(x => x.CapacityKW);
                //    double bmsCapacity = bms_list.Sum(x => x.CapacityKW);
                //    double pvCapacity = pv_list.Sum(x => x.CapacityKW);
                //    row.Add("TotalPcsCapacity", pcsCapacity);
                //    row.Add("TotalBmsCapacity", bmsCapacity);
                //    row.Add("TotalPvCapacity", pvCapacity);
                //    row.Add("PcsCount", pcs_list.Count());
                //    row.Add("BmsCount", bms_list.Count());
                //    row.Add("PvCount", pv_list.Count());
                //    array.Add(row);

                //}
                return Ok(source);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        //[HttpGet("gettemporarycontractorusers")]
        //public async Task<IActionResult> GetTemporaryContractorUsers()
        //{
        //    return Ok(_accountContext.VwContractorusers);
        //}


        [HttpGet("getcontractassetbyfindsiteid")]
        public async Task<IActionResult> GetContractAssetByFindSiteId(int siteid)
        {
            var result = _accountContext.ContractorAssets.Where(x => x.SiteId == siteid);
            return Ok(result);
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
    }
}
