using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PEIU.Models;
using PEIU.Models.IdentityModel;
using PEIU.Models.Database;
using PEIU.Models.ExchangeModel;
using PEIU.Service.WebApiService;
using PEIU.Service.WebApiService.Localization;
using PEIU.Service.WebApiService.Publisher;
using PES.Toolkit.Auth;
using PEIU.Models.DatabaseModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Criterion;
using NHibernate;
using System.Transactions;

namespace WebApiService.Controllers
{
    [Route("api/auth")]
    [Authorize]
    [ApiController]
    //[RequireHttps]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<UserAccountEF> _userManager;
        //AccountDataContext _accountContext;
        private readonly IStringLocalizer<LocalizedIdentityErrorDescriber> _localizer;
        private readonly SignInManager<UserAccountEF> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<Role> roleManager;
        private readonly IHTMLGenerator htmlGenerator;
        readonly ReservedRegisterNotifyPublisher Publisher;
        private readonly IClaimServiceFactory _claimsManager;
        private readonly ILogger<StatisticsController> logger;
        readonly AccountDataContext _accountContext;
        readonly AccountEF accountEF;

        public AuthController(UserManager<UserAccountEF> userManager,
            SignInManager<UserAccountEF> signInManager, RoleManager<Role> _roleManager, ILogger<StatisticsController> logger,
            IEmailSender emailSender, IHTMLGenerator _htmlGenerator, IClaimServiceFactory claimsManager, AccountEF accountEF,
            IStringLocalizer<LocalizedIdentityErrorDescriber> localizer, ReservedRegisterNotifyPublisher _publisher,
            AccountDataContext _accountContext
            /*,AccountDataContext accountContext*/)
        {
            _userManager = userManager;
            //_accountContext = accountContext;
            _signInManager = signInManager;
            _localizer = localizer;
            _emailSender = emailSender;
            htmlGenerator = _htmlGenerator;
            Publisher = _publisher;
            roleManager = _roleManager;
            _claimsManager = claimsManager;
            this.logger = logger;
            this._accountContext = _accountContext;
            this.accountEF = accountEF;
        }

        //private Task<IActionResult> RunAccountContextAsync(Func<AccountEF, Task<IActionResult>> context)
        //{
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        var otherOfferService = scope.ServiceProvider.GetService<AccountEF>();
        //        // everything was ok here. then I did: 
        //        return context.Invoke(otherOfferService);
        //    }
        //}

        [HttpPost, Route("logout")]
        public async Task<IActionResult> LogOff()
        {
            Console.WriteLine("logout~");
            await _signInManager.SignOutAsync();
            //_logger.LogInformation(4, "User logged out.");
            return Ok();
        }

        [HttpPost, Route("logintoredirect")]
        public async Task<IActionResult> LoginToRedirect(string ReturnUrl = null)
        {
            Console.WriteLine("ReturnUrl : " + ReturnUrl);
            return Ok(StatusCodes.Status200OK);
        }


        [HttpGet, Route("me")]
        public async Task<IActionResult> Me(string redirecturl = null)
        {
            
            Console.WriteLine("Me~Me~Me");
            var user = _claimsManager.FindUserAccount(HttpContext.User);
            if (user != null)
            {
                return Ok(new { Result = IdentityResult.Success, User = user });
            }
            else
            {
                IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).UserNotFound();
                IdentityResult _result = IdentityResult.Failed(error);
                return BadRequest(new { Result = _result });
            }
            //Console.WriteLine("Call the me");
            //Console.WriteLine($"redirect Url : " + redirecturl);
            //JObject data = new JObject();
            //data.Add("id", 3);
            //data.Add("username", "최고은");
            //data.Add("email", "ccc@ccc.com");
            //data.Add("created_at", DateTime.Now);
            //data.Add("updated_at", DateTime.Now);

            //JObject root = new JObject();
            //root.Add("status", "success");
            //root.Add("data", data);
            //return Ok(root);
        }

        [HttpGet, Route("forgotpassword"), AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var account = await _userManager.FindByEmailAsync(email);
            if (account == null)
            {
                IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).UserNotFound();
                IdentityResult _result = IdentityResult.Failed(error);
                return BadRequest(new { Result = _result });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(account);
            return Ok(new { Result = IdentityResult.Success, Token = token });
        }

        [HttpPost, Route("resetpassword"), AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var account = await _userManager.FindByEmailAsync(model.Email);
                if (account == null)
                {
                    IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).UserNotFound();
                    IdentityResult _result = IdentityResult.Failed(error);
                    return BadRequest(new { Result = _result });
                }

                var result = await _userManager.ResetPasswordAsync(account, model.Token, model.NewPassword);
                return Ok(new { Result = result } );
            }
            else
            {
                return BadRequest(StatusCodes.Status400BadRequest);
            }
        }

        /// <summary>
        /// 로그인
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Todo
        ///     {
        ///        "Email": "cge@power21.co.kr",
        ///        "Password": "*****",
        ///        "RememberMe": "False"
        ///     }
        ///
        /// </remarks>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        [HttpPost, Route("login"), AllowAnonymous]
        public async Task<IActionResult> Login([FromBody]LoginModel loginModel)
        {
            Console.WriteLine("Call the login. correct");
            //return Ok();
            return await OldLogin(loginModel);
        }

        [HttpPost, Route("login2"), AllowAnonymous]
        public async Task<IActionResult> Login2()
        {
            Console.WriteLine("Call the login2. No Parameter. correct");
            return Ok();
        }


        //[HttpPost, Route("login"), AllowAnonymous]
        ////public async Task<IActionResult> Login([FromBody]JObject jo)
        //public async Task<IActionResult> Login()
        //{
        //    Console.WriteLine("call login~. No Parameter");
        //    //foreach (string key in Response.Headers.Keys)
        //    //{
        //    //    Console.WriteLine($"{key} : {Response.Headers[key]}");
        //    //}
        //    //Response.Cookies.Append("babo", "you~");
        //    //if (jo == null)
        //    //    return NoContent();
        //    //return await ClaimsLogin(jo);
        //    return Ok();
        //}

        //private async Task<IActionResult> ClaimsLogin([FromBody]JObject jo)
        //{
        //    bool isUservalid = false;
        //    LoginViewModel user = JsonConvert.DeserializeObject<LoginViewModel>(jo.ToString());

        //    if (ModelState.IsValid && isUservalid)
        //    {
        //        var claims = new List<Claim>();

        //        claims.Add(new Claim(ClaimTypes.Name, user.Email));


        //        var identity = new ClaimsIdentity(
        //            claims, JwtBearerDefaults.AuthenticationScheme);

        //        var principal = new ClaimsPrincipal(identity);

        //        var props = new AuthenticationProperties();
        //        props.IsPersistent = user.RememberMe;

        //        HttpContext.SignInAsync(
        //            IdentityConstants.ApplicationScheme,
        //            principal, props).Wait();
        //        string token = JasonWebTokenManager.GenerateToken(user.Email);
        //        return Ok(new { Token = token });
        //    }
        //    else
        //    {
        //        return BadRequest();
        //    }
        //}

        private async Task<IActionResult> OldLogin([FromBody]LoginModel jo)
        {
            //[FromBody]
            //JObject jo = null;
            Console.WriteLine($"Try Logging... {jo}");


            //UserAccount user = await _userManager.FindByNameAsync(input_user.Email);

            if (ModelState.IsValid)
            {
                Console.WriteLine($"Model State is Valid");
                Console.WriteLine(jo.ToString());
                UserAccountEF account = await _userManager.FindByEmailAsync(jo.Email);
                if (account == null)
                {
                    IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).UserNotFound();
                    IdentityResult _result = IdentityResult.Failed(error);
                    return BadRequest(new { Result = _result });
                }
                else if (account.SignInConfirm == false)
                {
                    IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).SignInNotConfirm(jo.Email);
                    IdentityResult _result = IdentityResult.Failed(error);
                    return BadRequest(new { Result = _result });
                }

                Microsoft.AspNetCore.Identity.SignInResult signResult = await _signInManager.PasswordSignInAsync(jo.Email, jo.Password, true, false);
                if (signResult.Succeeded)
                {
                    using (NHibernate.ISession session = _accountContext.SessionFactory.OpenSession())
                    {
                        var accountUser = await _userManager.FindByEmailAsync(jo.Email);
                        IList<Claim> claims = await _userManager.GetClaimsAsync(accountUser);
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, accountUser.Id));
                        claims.Add(new Claim(ClaimTypes.Email, accountUser.Email));
                        claims.Add(new Claim(ClaimTypes.Name, accountUser.FirstName + accountUser.LastName));
                        if (accountUser.UserType == RegisterType.Aggregator)
                        {
                            VwAggregatoruser agg = await session.GetAsync<VwAggregatoruser>(accountUser.Id);
                            claims.Add(new Claim(UserClaimTypes.AggregatorGroupIdentifier, agg.AggGroupId));
                            claims.Add(new Claim(ClaimTypes.Role, UserRoleTypes.Aggregator));
                            if (string.IsNullOrEmpty(agg.AggName) == false)
                                claims.Add(new Claim(UserClaimTypes.AggregatorGroupName, agg.AggName));

                            //var agg = _accountContext.VwAggregatorusers.FirstOrDefault(x => x.UserId == accountUser.Id);
                            //claims.Add(new Claim(UserClaimTypes.AggregatorGroupIdentifier, agg.AggGroupId));
                            //claims.Add(new Claim(ClaimTypes.Role, UserRoleTypes.Aggregator));
                            //if (string.IsNullOrEmpty(agg.AggName) == false)
                            //    claims.Add(new Claim(UserClaimTypes.AggregatorGroupName, agg.AggName));
                        }
                        else if (accountUser.UserType == RegisterType.Contrator)
                        {
                            VwContractoruser con = await session.GetAsync<VwContractoruser>(accountUser.Id);
                            claims.Add(new Claim(UserClaimTypes.AggregatorGroupIdentifier, con.AggGroupId));
                            claims.Add(new Claim(ClaimTypes.Role, UserRoleTypes.Contractor));
                            if (string.IsNullOrEmpty(con.AggName) == false)
                                claims.Add(new Claim(UserClaimTypes.AggregatorGroupName, con.AggName));

                            //var contractor = _accountContext.VwContractorusers.FirstOrDefault(x => x.UserId == accountUser.Id);
                            //claims.Add(new Claim(UserClaimTypes.AggregatorGroupIdentifier, contractor.AggGroupId));
                            //claims.Add(new Claim(ClaimTypes.Role, UserRoleTypes.Contractor));
                            //if (string.IsNullOrEmpty(contractor.AggName) == false)
                            //    claims.Add(new Claim(UserClaimTypes.AggregatorGroupName, contractor.AggName));
                        }
                        else if (accountUser.UserType == RegisterType.Supervisor)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, UserRoleTypes.Supervisor));
                        }


                        string token = JasonWebTokenManager.GenerateToken(jo.Email, UserClaimTypes.Issuer, claims);

                        //if (string.IsNullOrEmpty(returnUrl) == false)
                        //{
                        //    Console.WriteLine("returnurl:" + returnUrl);
                        //    return Redirect(returnUrl);
                        //}
                        Console.WriteLine("Log-in Success: " + jo.Email);
                        return Ok(new { Result = signResult, Token = token, User = accountUser });
                    }
                }
                else
                {
                    Console.WriteLine($"Login Failed");
                    //if (signResult.RequiresTwoFactor)
                    //{
                    //    return RedirectToAction("act", new { ReturnUrl = returnUrl, RememberMe = user.RememberMe });
                    //}
                    if (signResult.IsLockedOut)
                    {
                        IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).UserLockoutEnabled();
                        IdentityResult _result = IdentityResult.Failed(error);
                        return BadRequest(new { Result = _result });
                    }
                    else
                    {
                        IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).PasswordMismatch();
                        IdentityResult _result = IdentityResult.Failed(error);
                        return BadRequest(new { Result = _result });
                    }
                }


            }
            else
            {
                Console.WriteLine("Invalid LoginViewModel");
                return Ok(StatusCodes.Status406NotAcceptable);
            }
        }



        //[HttpPost, Route("signonaggregatorgroup")]
        //[AllowAnonymous]
        ////[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SignonAggregatorGroup([FromBody] AggregatorGroupRegistModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if(_accountContext.AggregatorGroups.Any(x=>x.AggName == model.AggregatorGroupName))
        //        {
        //            IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).TargetAggGroupHasAlready(model.AggregatorGroupName);
        //            IdentityResult _result = IdentityResult.Failed(error);
        //            return BadRequest(new { Result = _result });
        //        }
        //        AggregatorGroup aggregatorGroup = new AggregatorGroup();
        //        aggregatorGroup.ID = Guid.NewGuid().ToString();
        //        aggregatorGroup.AggName = model.AggregatorGroupName;
        //        aggregatorGroup.Representation = model.Represenation;
        //        aggregatorGroup.Address = model.Address;
        //        aggregatorGroup.CreateDT = DateTime.Now;
        //        aggregatorGroup.PhoneNumber = model.PhoneNumber;
        //        await _accountContext.AddAsync(aggregatorGroup);
        //        await _accountContext.SaveChangesAsync();
        //        return Ok(aggregatorGroup.ID);
        //    }
        //    return BadRequest();
        //}

        //[HttpPost, Route("signonaggregator")]
        //[AllowAnonymous]
        ////[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SignonAggregator([FromBody] AggregatorRegistModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        AggregatorGroup aggregatorGroup = _accountContext.AggregatorGroups.FirstOrDefault(x => x.AggName == model.Company);
        //        if (aggregatorGroup == null)
        //        {
        //            aggregatorGroup = new AggregatorGroup();
        //            aggregatorGroup.ID = Guid.NewGuid().ToString();
        //            aggregatorGroup.AggName = model.Company;
        //            aggregatorGroup.Representation = "";
        //            aggregatorGroup.Address = model.Address;
        //            aggregatorGroup.CreateDT = DateTime.Now;
        //            aggregatorGroup.PhoneNumber = model.PhoneNumber;
        //            await _accountContext.AddAsync(aggregatorGroup);
        //        }

        //        var user = CreateUserAccount(model, RegisterType.Aggregator);
        //        var result = await _userManager.CreateAsync(user, model.Password);
        //        //result.Errors
        //        if (result.Succeeded)
        //        {

        //            await _accountContext.SaveChangesAsync();

        //            string email_contents = htmlGenerator.GenerateHtml("NotifyEmail.html",
        //                    new
        //                    {
        //                        Name = $"{user.FirstName} {user.LastName}",
        //                        Company = model.Company,
        //                        Email = model.Email,
        //                        Phone = model.PhoneNumber,
        //                        Address = model.Address,
        //                        Aggregator = aggregatorGroup.AggName
        //                    });
        //            string sender = "PEIU 운영팀";
        //            var aggregator_account_users = await _userManager.GetUsersInRoleAsync(UserRoleTypes.Supervisor);
        //            await _emailSender.SendEmailAsync(sender, "새로운 중계거래자 가입이 요청되었습니다", email_contents, aggregator_account_users.Select(x => x.Email).ToArray());
        //            return Ok(new { Result = result });
        //        }
        //        else
        //        {
        //            return BadRequest(new { Result = result });
        //        }
        //    }
        //    return BadRequest();
        //}

        //[HttpPost, Route("signonsupervisor")]
        //[AllowAnonymous]
        ////[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SignonSupervisor([FromBody] SupervisorRegistModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = CreateUserAccount(model);
        //        var result = await _userManager.CreateAsync(user, model.Password);
        //        //result.Errors
        //        if (result.Succeeded)
        //        {
        //            var role_add_result = await _userManager.AddToRoleAsync(user, UserRoleTypes.Supervisor);
        //            //_userManager.AddClaimAsync(user, new Claim())
        //            SupervisorUser aggregatorUser = new SupervisorUser();
        //            aggregatorUser.UserId = user.Id;
        //            await _accountContext.SupervisorUsers.AddAsync(aggregatorUser);
        //            await _accountContext.SaveChangesAsync();
        //            return Ok(new { Result = result });
        //        }
        //        else
        //        {
        //            return BadRequest(new { Result = result });
        //        }
        //    }
        //    return BadRequest();
        //}

        [HttpPost, Route("signoncontractor")]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SignonContractor([FromBody] AggregatorRegistModelBase model)
        {
            //TransactionScope trans_scope = new TransactionScope(System.Transactions.TransactionScopeAsyncFlowOption.Enabled);
            var trans_scope = await accountEF.Database.BeginTransactionAsync();
            
            try

            {
                
                if (ModelState.IsValid)
                {
                    using (NHibernate.ISession session = _accountContext.SessionFactory.OpenSession())
                    using (ITransaction trans = session.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        AggregatorGroup aggregatorGroup = await session.GetAsync<AggregatorGroup>(model.AggregatorGroupId);
                        if (aggregatorGroup == null)
                        {
                            if (model.Type == RegisterType.Contrator)
                            {
                                IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).AggregatorNotFounded(model.AggregatorGroupId);
                                IdentityResult _result = IdentityResult.Failed(error);
                                return base.BadRequest(new { Result = _result });
                            }
                            else
                            {
                                var groups = await session.CreateCriteria<AggregatorGroup>()
                                 .Add(Restrictions.Eq("AggName", model.Company))
                                 .ListAsync<AggregatorGroup>();
                                aggregatorGroup = groups.FirstOrDefault();
                                if (aggregatorGroup == null)
                                {
                                    aggregatorGroup = new AggregatorGroup();
                                    aggregatorGroup.ID = Guid.NewGuid().ToString();
                                    aggregatorGroup.AggName = model.Company;
                                    aggregatorGroup.Representation = "";
                                    aggregatorGroup.Address = model.Address;
                                    aggregatorGroup.CreateDT = DateTime.Now;
                                    aggregatorGroup.PhoneNumber = model.PhoneNumber;
                                    await session.SaveAsync(aggregatorGroup);
                                }
                            }
                        }


                        var user = CreateUserAccount(model);

                        JObject obj = JObject.FromObject(user);
                        var result = await _userManager.CreateAsync(user, model.Password);
                        //result.Errors
                        if (result.Succeeded)
                        {
                            
                            RegisterAccount(session, user, model.AggregatorGroupId);
                            RegisterFileRepositary registerModel = RegisterFile(session, user.Id, model.RegisterFilename, model.RegisterFilebase64);
                            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                            await Publisher.PublishMessageAsync(obj.ToString(), cancellationTokenSource.Token);
                            logger.LogInformation($"회원 가입 성공: {obj}");
                            if (model.NotifyEmail)
                            {
                                string email_contents = htmlGenerator.GenerateHtml("NotifyEmail.html",
                                    new
                                    {
                                        Name = $"{user.FirstName} {user.LastName}",
                                        Company = model.Company,
                                        Email = model.Email,
                                        Phone = model.PhoneNumber,
                                        Address = model.Address,
                                        Aggregator = aggregatorGroup.AggName
                                    });

                                string sender = "PEIU 운영팀";
                                string target = "";

                                List<string> supervisor_emails = (await _userManager.GetUsersInRoleAsync(UserRoleTypes.Supervisor)).Select(x => x.Email).ToList();
                                if (model.Type == RegisterType.Aggregator)
                                {
                                    target = "중개거래사업자";
                                }
                                else if (model.Type == RegisterType.Contrator)
                                {
                                    target = "발전사업자";
                                    var agg_result = await session.CreateCriteria<VwAggregatoruser>()
                                        .Add(Restrictions.Eq("AggGroupId", model.AggregatorGroupId))
                                        .ListAsync<VwAggregatoruser>();
                                    supervisor_emails.AddRange(agg_result.Select(x => x.Email));

                                    //targetEmailUsers = await _userManager.GetUsersInRoleAsync(UserRoleTypes.Aggregator);
                                    //targetEmailUsers = targetEmailUsers.Where(x=>x.agg)
                                }
                                else if (model.Type == RegisterType.Supervisor)
                                {
                                    target = "관리자";
                                }

                                //var aggregator_account_users = await _userManager.GetUsersInRoleAsync(UserRoleTypes.Supervisor);
                                await _emailSender.SendEmailAsync(sender, $"새로운 {target} 가입이 요청되었습니다", email_contents, registerModel, supervisor_emails.ToArray());
                                logger.LogInformation($"가입 알림 메일 전송: {string.Join(", ", supervisor_emails)}");
                            }
                            //throw new Exception();
                            await trans.CommitAsync();
                            //trans.Commit();
                            trans_scope.Commit();
                            return Ok(new { Result = result });
                        }
                        else
                        {
                            trans_scope.Dispose();
                            return BadRequest(new { Result = result });
                        }
                    }
                }
                else
                {
                    trans_scope.Dispose();
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                trans_scope.Dispose();
                logger.LogError(ex, ex.Message);
                return BadRequest();
            }
        
        }

        ///// <summary>
        ///// 임시 사이트 추가
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //[HttpPost, Route("registtemporarysite")]
        //[AllowAnonymous]
        ////[ValidateAntiForgeryToken]
        //public async Task<IActionResult> RegistTemporarySite([FromBody] RegisterSiteModel model)
        //{

        //    //model.Email = value["Email"].ToString();
        //    //model.Password = value["Password"].ToString();
        //    //model.ConfirmPassword = value["ConfirmPassword"].ToString();
        //    //model.Username
        //    if (ModelState.IsValid)
        //    {
                
        //        UserAccount contractor = await _userManager.FindByEmailAsync(model.ContractorEmail);
        //        if(contractor == null)
        //        {
        //            IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).ContractorNotFounded(model.ContractorEmail);
        //            IdentityResult _result = IdentityResult.Failed(error);
        //            return BadRequest(new { Result = _result });
        //        }
        //        ContractorUser cu = await _accountContext.ContractorUsers.FindAsync(contractor.Id);
                
        //        TemporaryContractorSite newSite = new TemporaryContractorSite();
        //        newSite.Address1 = model.Address1;
        //        newSite.Address2 = model.Address2;
        //        newSite.ContractUserId = contractor.Id;
        //        newSite.Latitude = model.Latitude;
        //        newSite.IsJeju = model.IsJejuSite;
        //        newSite.Longtidue = model.Longtidue;
        //        newSite.LawFirstCode = model.LawFirstCode;
        //        newSite.LawMiddleCode = model.LawMiddleCode;
        //        newSite.LawLastCode = model.LawLastCode;
        //        newSite.ServiceCode = model.ServiceCode;
        //        newSite.RegisterTimestamp = DateTime.Now;

        //        foreach (RegisterAssetModel asset in model.Assets)
        //        {
        //            string assetName = $"{asset.Type}{asset.Index}";
        //            TemporaryContractorAsset newAsset = new TemporaryContractorAsset();
        //            newAsset.AssetName = assetName;
        //            newAsset.AssetType = asset.Type;
        //            newAsset.CapacityKW = asset.CapacityMW;
        //            newAsset.ContractorSite = newSite;
        //            newAsset.UniqueId = Guid.NewGuid().ToString();
        //            await _accountContext.TemporaryContractorAssets.AddAsync(newAsset);
        //        }

        //        await _accountContext.TemporaryContractorSites.AddAsync(newSite);
        //        await _accountContext.SaveChangesAsync();
        //        //var user = new ReservedAssetLocation
        //        //{
        //        //    AccountId = model.AccountId,
        //        //    Address1 = model.Address1,
        //        //    Address2 = model.Address2,
        //        //    ControlOwner = model.ControlOwner,
        //        //    //SiteInformation = model.SiteInformation,
        //        //    RegisterTimestamp = DateTime.Now,
        //        //    LawFirstCode = model.LawFirstCode,
        //        //    LawMiddleCode = model.LawMiddleCode,
        //        //    LawLastCode = model.LawLastCode,
        //        //    ServiceCode = model.ServiceCode,
        //        //    Latitude = model.Latitude,
        //        //    Longtidue = model.Longtidue
        //        //};
        //        //accountContext.ReservedAssetLocations.Add(user);
        //        //await accountContext.SaveChangesAsync();
        //        return Ok();
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return BadRequest();
        //}

        private UserAccountEF CreateUserAccount(AggregatorRegistModelBase model)
        {
            var user = new UserAccountEF
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                CompanyName = model.Company,
                NormalizedUserName = model.Email.ToUpper(),
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                RegistDate = DateTime.Now,
                Expire = DateTime.Now.AddDays(14),
                UserType = model.Type
            };
            return user;
        }

        private RegisterFileRepositary RegisterFile(NHibernate.ISession session, string userId, string fileName, string FileBase64Data)
        {
            if (string.IsNullOrEmpty(FileBase64Data))
                return null;
            RegisterFileRepositary registerFile = new RegisterFileRepositary();
            registerFile.UserId = userId;
            registerFile.FileName = fileName;
            string[] splits = FileBase64Data.Split(',');
            registerFile.ContentsType = splits[0];
            byte[] data = Convert.FromBase64String(splits[1]);
            registerFile.Contents = data;
            registerFile.CreateDT = DateTime.Now.Date;
            session.Save(registerFile);
            return registerFile;
        }

        private void RegisterAccount(NHibernate.ISession session, UserAccountEF user, string aggregatorGroupId)
        {
            switch(user.UserType)
            {
                case RegisterType.Contrator:
                    ContractorUser cu = new ContractorUser();
                    cu.AggGroupId = aggregatorGroupId;
                    cu.ContractStatus = (int)ContractStatusCodes.Signing;
                    cu.UserId = user.Id;
                    session.SaveAsync(cu);
                    Task t = _userManager.AddToRoleAsync(user, UserRoleTypes.Contractor);
                    t.Wait();
                    break;
                //case RegisterType.Supervisor:
                //    var role_add_result = await _userManager.AddToRoleAsync(user, UserRoleTypes.Supervisor);
                //    //_userManager.AddClaimAsync(user, new Claim())
                //    SupervisorUser supervisorUser = new SupervisorUserEF();
                //    supervisorUser.UserId = user.Id;
                //    await _accountContext.SupervisorUsers.AddAsync(supervisorUser);
                //    break;
                case RegisterType.Aggregator:
                    Task ta =  _userManager.AddToRoleAsync(user, UserRoleTypes.Aggregator);
                    ta.Wait();
                    //_userManager.AddClaimAsync(user, new Claim())
                    AggregatorUser aggregatorUser = new AggregatorUser();
                    aggregatorUser.AggGroupId = aggregatorGroupId;
                    aggregatorUser.UserId = user.Id;
                    session.SaveAsync(aggregatorUser);
                    break;
            }
        }
    }
}