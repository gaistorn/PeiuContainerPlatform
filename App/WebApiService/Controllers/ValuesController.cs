using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using PEIU.Models;

namespace WebApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        UserManager<AccountModel> _userManager;
        public ValuesController(UserManager<AccountModel> userManager)
        {
            _userManager = userManager;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
       
        [Authorize(Policy = PEIU.Models.CommonClaimTypes.READ_CUSTOMER_INFO_CLAIM)]
        [HttpGet("getclaim")]        
        public async Task<ActionResult> getclaim()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var claims = await _userManager.GetClaimsAsync(user);
            return Ok(claims);
        }

        // GET api/values/5
        [Authorize(Policy = PEIU.Models.CommonClaimTypes.CREATE_CUSTOMER_INFO_CLAIM)]
        [HttpGet("getclaim2")]
        public async Task<ActionResult> getclaim2()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var claims = await _userManager.GetClaimsAsync(user);
            return Ok(claims);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
