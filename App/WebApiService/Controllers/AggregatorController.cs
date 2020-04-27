using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PEIU.Models.Database;
using PEIU.Models.ExchangeModel;
using PEIU.Models.IdentityModel;
using PeiuPlatform.App;
using PeiuPlatform.App.Localization;

namespace WebApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AggregatorController : ControllerBase
    {
        readonly AccountEF accountContext;
        readonly UserManager<UserAccountEF> _userManager;
        readonly IEmailSender _emailSender;
        readonly IClaimServiceFactory _claimsManager;
        public AggregatorController(AccountEF _accountContext, UserManager<UserAccountEF> userManager, IEmailSender emailSender, IClaimServiceFactory claimsManager)
        {
            accountContext = _accountContext;
            _userManager = userManager;
            _emailSender = emailSender;
            _claimsManager = claimsManager;
        }

        [Authorize(Policy = UserPolicyTypes.RequiredManager)]
        [HttpPost, Route("submituserstatus")]
        public async Task<IActionResult> SubmitUserStatus([FromBody]SubmitUserStatusModel model)
        {
            if (ModelState.IsValid)
            {
                string name = _claimsManager.GetClaimsValue(HttpContext.User, ClaimTypes.Name);
                var user_account = await accountContext.ContractorUsers.FindAsync(model.UserId);
                var user = await accountContext.VwContractorusers.FindAsync(model.UserId);
                if(user_account == null)
                {
                    IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).UserNotFound();
                    IdentityResult _result = IdentityResult.Failed(error);
                    return BadRequest(new { Result = _result });
                }
                user_account.ContractStatus = model.Status;
                if(model.Status == PEIU.Models.ContractStatusCodes.Activating)
                {
                    var pk = await accountContext.Users.FindAsync(model.UserId);
                    pk.SignInConfirm = true;
                    string sender = $"{user.AggName} 운영팀";
                    await _emailSender.SendEmailAsync(sender, $"가입을 축하합니다. 승인날짜:{DateTime.Now}", $"{pk.FirstName + pk.LastName} 고객님이 회원가입이 정상적으로 처리되었습니다", pk.Email);

                }
                await accountContext.SaveChangesAsync();
                if (model.Notification)
                {
                    IEnumerable<string> email_address = null;
                    if (HttpContext.User.IsInRole(UserRoleTypes.Supervisor))
                    {
                        var targets = await _userManager.GetUsersInRoleAsync(UserRoleTypes.Supervisor);
                        email_address = targets.Select(x => x.Email);
                    }
                    else
                    {
                       string group_id = _claimsManager.GetClaimsValue(HttpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                       
                        var role_user = accountContext.VwAggregatorusers.Where(x => x.AggGroupId == group_id);
                        email_address = role_user.Select(x => x.Email);
                    }
                    string senderName = $"{name} (운영팀)";
                    try

                    {
                        await _emailSender.SendEmailAsync(senderName, $"{name} 고객님의 새로운 결제가 확인되었습니다", $"대상 고객: {name}\n결제 내용: {model.Status}\n결제 사유: {model.Reason}", email_address.ToArray());
                    }
                    catch (Exception ex)
                    {

                    }
                }
                
            }
            return Ok();
        }
        
        [HttpGet, Route("getaggregatorgroups")]
        [Authorize(Policy = UserPolicyTypes.RequiredManager)]
        public async Task<IActionResult> GetAggregatorGroups()
        {
            try
            {
                if (HttpContext.User.IsInRole(UserRoleTypes.Supervisor))
                {
                    return Ok(accountContext.AggregatorGroups);
                }
                else if (HttpContext.User.IsInRole(UserRoleTypes.Aggregator))
                {
                    string agg_group_id = _claimsManager.GetClaimsValue(HttpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                    return Ok(accountContext.AggregatorGroups.FindAsync(agg_group_id));
                }
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet, Route("getaggregatorusers")]
        [Authorize(Policy = UserPolicyTypes.RequiredManager)]
        public async Task<IActionResult> GetAggregatorUsers(string aggregatorgroupid = null)
        {
            try
            {
                if (HttpContext.User.IsInRole(UserRoleTypes.Supervisor))
                {
                    if (string.IsNullOrEmpty(aggregatorgroupid))
                        return Ok(accountContext.VwAggregatorusers);
                    else
                        return Ok(accountContext.VwAggregatorusers.Where(x => x.AggGroupId == aggregatorgroupid));
                }
                else if (HttpContext.User.IsInRole(UserRoleTypes.Aggregator))
                {
                    string agg_group_id = _claimsManager.GetClaimsValue(HttpContext.User, UserClaimTypes.AggregatorGroupIdentifier);
                    return Ok(accountContext.VwAggregatorusers.Where(x => x.AggGroupId == agg_group_id));
                }
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
                
}