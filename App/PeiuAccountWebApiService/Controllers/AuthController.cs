using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using PeiuPlatform.App.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PeiuPlatform.Model;
using PeiuPlatform.Model.IdentityModel;
using PeiuPlatform.Model.Database;
using PeiuPlatform.Model.ExchangeModel;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace PeiuPlatform.App.Controllers
{
    [Route("api/auth")]
    [Authorize]
    [ApiController]
    //[RequireHttps]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<UserAccountEF> _userManager;
        AccountEF _accountContext;
        private readonly IStringLocalizer<LocalizedIdentityErrorDescriber> _localizer;
        private readonly SignInManager<UserAccountEF> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<Role> roleManager;
        private readonly IHTMLGenerator htmlGenerator;
        private readonly ILogger<AuthController> logger;
        private readonly IClaimServiceFactory _claimsManager;
        private readonly IMemoryCache distributedCache;
        public const string SiteIdsByRccClaim = "SiteIdsByRcc";
        private readonly string WebServerUrl;

        public AuthController(UserManager<UserAccountEF> userManager,
            SignInManager<UserAccountEF> signInManager, RoleManager<Role> _roleManager, IMemoryCache distributedCache,
            IEmailSender emailSender, IHTMLGenerator _htmlGenerator, IClaimServiceFactory claimsManager, ILogger<AuthController> logger,
            IStringLocalizer<LocalizedIdentityErrorDescriber> localizer,  IConfiguration config,
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
            this.distributedCache = distributedCache;
            WebServerUrl = config.GetSection("WebServerUrl").Value;
            this.logger = logger;
        }

        [HttpPost, Route("logout")]
        public async Task<IActionResult> LogOff()
        {
            Console.WriteLine("logout~");
            await _signInManager.SignOutAsync();
            //_logger.LogInformation(4, "User logged out.");
            return Ok();
        }

        [HttpPost, Route("logintoredirect")]
        public IActionResult LoginToRedirect(string ReturnUrl = null)
        {
            Console.WriteLine("ReturnUrl : " + ReturnUrl);
            return Ok(StatusCodes.Status200OK);
        }


        [HttpGet, Route("me")]
        public async Task<IActionResult> Me(string redirecturl = null)
        {
            try
            {
                Console.WriteLine("Me~Me~Me");
                var user = await _claimsManager.FindUserAccount(HttpContext.User);
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
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest();
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

        [HttpGet, Route("requestaccesstoken"), AllowAnonymous]
        public async Task<IActionResult> RequestAccessTokenAsync(string Email, string RefreshToken)
        {
            string refreshToken = null;
            if (distributedCache.TryGetValue(Email, out refreshToken) == false)
            {
                return BadRequest("해당 Refresh Token은 존재하지 않거나 만료되었습니다. 새로 로그인하시기 바랍니다");
            }
            if (JasonWebTokenManager.ValidateRefreshToken(Email, RefreshToken) && refreshToken == RefreshToken)
            {
                var accountUser = await _userManager.FindByEmailAsync(Email);                
                string token = await GenerateAccessTokenAsync(accountUser);
                return Ok(new { AccessToken = token });
            }
            else
                return BadRequest("해당 Refresh Token은 유효한 Token이 아닙니다. ");
        }

        [HttpGet, Route("forgotpassword"), AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            var account = await _userManager.FindByEmailAsync(Email);
            if (account == null)
            {
                IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).UserNotFound();
                IdentityResult _result = IdentityResult.Failed(error);
                return BadRequest(new { Result = _result });
            }

            var Token = await _userManager.GeneratePasswordResetTokenAsync(account);
            Guid tokenId = Guid.NewGuid();
            string tokenUid = tokenId.ToString();
            //HttpContext.Session.SetString()

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5));
            distributedCache.Set(tokenUid, Token, cacheEntryOptions);

            //redisDb.StringSet(tokenUid, Token, expiry: TimeSpan.FromMinutes(30));

            string email_contents = htmlGenerator.GenerateHtml("ResetPassword.html",
                                new
                                {
                                    email = Email,
                                    token = tokenUid,
                                    url = WebServerUrl
                                });

            string sender = "PEIU 운영팀";

            //var aggregator_account_users = await _userManager.GetUsersInRoleAsync(UserRoleTypes.Supervisor);
            await _emailSender.SendEmailAsync(sender, $"비밀번호 초기화가 요청되었습니다", email_contents, Email);
            logger.LogInformation($"비밀번호 초기화 메일 전송: {Email}\n{Token}");
            return Ok(new { Result = IdentityResult.Success, Token = Token });
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

                string real_token = null;
                if (distributedCache.TryGetValue(model.Token, out real_token) == false)
                {
                    return BadRequest("초기화 시간이 5분이 지났거나, 잘못된 요청입니다. 다시 초기화하시기 바랍니다");
                }

                var result = await _userManager.ResetPasswordAsync(account, real_token, model.NewPassword);
                return Ok(new { Result = result });
            }
            else
            {
                return BadRequest(StatusCodes.Status400BadRequest);
            }
        }

#if DEBUG
        [HttpPost, Route("logintest"), AllowAnonymous]
        public async Task<IActionResult> LoginTest()
        {
            Console.WriteLine("Call the login. correct");
            LoginModel jo = new LoginModel();
            jo.Email = "power21@power21.co.kr";
            jo.Password = "power211219/";
            //return Ok();
            return await OldLogin(jo);
        }
#endif

        [HttpPost, Route("login"), AllowAnonymous]
        public async Task<IActionResult> Login([FromBody]LoginModel jo)
        {
            Console.WriteLine("Call the login. correct");
            //return Ok();
            return await OldLogin(jo);
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

        private async Task<IActionResult> OldLogin([FromBody]LoginModel user)
        {
            //[FromBody]
            //JObject jo = null;
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
                {
                    Console.WriteLine("Invalid User");
                    return BadRequest();
                }

                UserAccountEF account = await _userManager.FindByEmailAsync(user.Email);
                if (account == null)
                {
                    IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).UserNotFound();
                    IdentityResult _result = IdentityResult.Failed(error);
                    return BadRequest(new { Result = _result });
                }
                else if (account.SignInConfirm == false)
                {
                    IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).SignInNotConfirm(user.Email);
                    IdentityResult _result = IdentityResult.Failed(error);
                    return BadRequest(new { Result = _result });
                }
                else if (account.Expire <= DateTime.Now)
                {
                    IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).Expired();
                    IdentityResult _result = IdentityResult.Failed(error);
                    return BadRequest(new { Result = _result });
                }

                Microsoft.AspNetCore.Identity.SignInResult signResult = await _signInManager.PasswordSignInAsync(user.Email, user.Password, true, false);
                if (signResult.Succeeded)
                {
                    var accountUser = await _userManager.FindByEmailAsync(user.Email);
                    string token = await GenerateAccessTokenAsync(accountUser);
                    string result = JasonWebTokenManager.ValidateToken(user.Email, token, ClaimTypes.NameIdentifier);
                    string refreshToken = null;


                
                    if (distributedCache.TryGetValue(user.Email, out refreshToken) == false)
                    {
                        refreshToken = JasonWebTokenManager.GenerateRefreshToken(user.Email);
                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromDays(100));
                        distributedCache.Set(user.Email, refreshToken, cacheEntryOptions);
                    }
                    //if (string.IsNullOrEmpty(returnUrl) == false)
                    //{
                    //    Console.WriteLine("returnurl:" + returnUrl);
                    //    return Redirect(returnUrl);
                    //}
                    Console.WriteLine("Log-in Success: " + user.Email);
                    return Ok(new { Result = signResult, Token = token, RefreshToken = refreshToken, User = accountUser });
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

        private async Task<string> GenerateAccessTokenAsync(UserAccountEF userAccount)
        {
            var accountUser = userAccount;
            IList<Claim> claims = await _userManager.GetClaimsAsync(accountUser);
            claims.Add(new Claim(ClaimTypes.NameIdentifier, accountUser.Id));
            claims.Add(new Claim(ClaimTypes.Name, accountUser.FirstName + accountUser.LastName));

            string roleType = "";
            string id = "";
            if (accountUser.UserType == RegisterType.Aggregator)
            {
                roleType = UserRoleTypes.Aggregator;
                var agg = _accountContext.VwAggregatorusers.FirstOrDefault(x => x.UserId == accountUser.Id);
                id = agg.AggGroupId;
                claims.Add(new Claim(UserClaimTypes.AggregatorGroupIdentifier, agg.AggGroupId));
                //claims.Add(new Claim("", ))
                claims.Add(new Claim(ClaimTypes.Role, UserRoleTypes.Aggregator));
                if (string.IsNullOrEmpty(agg.AggName) == false)
                    claims.Add(new Claim(UserClaimTypes.AggregatorGroupName, agg.AggName));

            }
            else if (accountUser.UserType == RegisterType.Contrator)
            {
                var contractor = _accountContext.VwContractorusers.FirstOrDefault(x => x.UserId == accountUser.Id);
                id = accountUser.Id;
                claims.Add(new Claim(UserClaimTypes.AggregatorGroupIdentifier, contractor.AggGroupId));
                claims.Add(new Claim(ClaimTypes.Role, UserRoleTypes.Contractor));
                roleType = UserRoleTypes.Contractor;
                if (string.IsNullOrEmpty(contractor.AggName) == false)
                    claims.Add(new Claim(UserClaimTypes.AggregatorGroupName, contractor.AggName));
            }
            else if (accountUser.UserType == RegisterType.Supervisor)
            {
                claims.Add(new Claim(ClaimTypes.Role, UserRoleTypes.Supervisor));
                roleType = UserRoleTypes.Supervisor;
            }
            else if (accountUser.UserType == RegisterType.AccountManager)
            {
                claims.Add(new Claim(ClaimTypes.Role, UserRoleTypes.AccountManager));
            }
            else if (accountUser.UserType == RegisterType.Hubbub)
            {
                claims.Add(new Claim(ClaimTypes.Role, UserRoleTypes.HubbubUser));
            }

            bool IsPlatformUser =
                accountUser.UserType == RegisterType.Supervisor ||
                accountUser.UserType == RegisterType.Aggregator ||
                accountUser.UserType == RegisterType.Contrator;

            if (IsPlatformUser)
            {
                var siteids = ControlHelper.GetAvaliableRccCodes(_accountContext, roleType, id);
                claims.Add(new Claim(UserClaimTypes.SiteIdsByRccClaim, siteids));
            }

            string token = JasonWebTokenManager.GenerateToken(accountUser.Email, claims);
            return token;
        }

        private UserAccountEF CreateBasicAccount(BasicRegistModel model, RegisterType userType)
        {
            var user = new UserAccountEF
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                CompanyName = "파워이십일",
                NormalizedUserName = model.Email.ToUpper(),
                PhoneNumber = model.PhoneNumber,
                Address = "파워이십일",
                RegistDate = DateTime.Now,
                Expire = new DateTime(2100, 12, 31),
                UserType = userType,
                RegistrationNumber = ""
            };
            return user;
        }

        private UserAccountEF CreateSupervisorAccount(BasicRegistModel model)
        {
            var user = new UserAccountEF
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                CompanyName = "파워이십일",
                NormalizedUserName = model.Email.ToUpper(),
                PhoneNumber = model.PhoneNumber,
                Address = "대전광역시 유성과 관평동 1359번지 한신에스메카 609호",
                RegistDate = DateTime.Now,
                Expire = DateTime.Now.AddMonths(1),
                UserType = RegisterType.Supervisor,
                RegistrationNumber = "",
                SignInConfirm = true
            };
            return user;
        }

        private UserAccountEF CreateUserAccount(RegisterViewModel model, RegisterType type)
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
                UserType = type,
                RegistrationNumber = model.RegisterNumber
            };
            return user;
        }

        [HttpPost, Route("signonaggregatorgroup")]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SignonAggregatorGroup([FromBody] AggregatorGroupRegistModel model)
        {
            if (ModelState.IsValid)
            {
                if (_accountContext.AggregatorGroups.Any(x => x.AggName == model.AggregatorGroupName))
                {
                    IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).TargetAggGroupHasAlready(model.AggregatorGroupName);
                    IdentityResult _result = IdentityResult.Failed(error);
                    return BadRequest(new { Result = _result });
                }
                AggregatorGroupEF aggregatorGroup = new AggregatorGroupEF();
                aggregatorGroup.ID = Guid.NewGuid().ToString();
                aggregatorGroup.AggName = model.AggregatorGroupName;
                aggregatorGroup.Representation = model.Represenation;
                aggregatorGroup.Address = model.Address;
                aggregatorGroup.CreateDT = DateTime.Now;
                aggregatorGroup.PhoneNumber = model.PhoneNumber;
                await _accountContext.AddAsync(aggregatorGroup);
                await _accountContext.SaveChangesAsync();
                return Ok(aggregatorGroup.ID);
            }
            return BadRequest();
        }

        [HttpPost, Route("signonaggregator")]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SignonAggregator([FromBody] AggregatorRegistModelBase model)
        {
            if (ModelState.IsValid)
            {
                var trans = await _accountContext.Database.BeginTransactionAsync();
                try

                {
                    AggregatorGroupEF aggregatorGroup = _accountContext.AggregatorGroups.FirstOrDefault(x => x.AggName == model.Company);
                    if (aggregatorGroup == null)
                    {
                        aggregatorGroup = new AggregatorGroupEF();
                        aggregatorGroup.ID = Guid.NewGuid().ToString();
                        aggregatorGroup.AggName = model.Company;
                        aggregatorGroup.Representation = "";
                        aggregatorGroup.Address = model.Address;
                        aggregatorGroup.CreateDT = DateTime.Now;
                        aggregatorGroup.PhoneNumber = model.PhoneNumber;
                        await _accountContext.AddAsync(aggregatorGroup);
                    }

                    var user = CreateUserAccount(model, RegisterType.Aggregator);
                    var result = await _userManager.CreateAsync(user, model.Password);
                    //result.Errors
                    if (result.Succeeded)
                    {
                        var role_add_result = await _userManager.AddToRoleAsync(user, UserRoleTypes.Aggregator);
                        //_userManager.AddClaimAsync(user, new Claim())
                        AggregatorUserEF aggregatorUser = new AggregatorUserEF();
                        aggregatorUser.AggregatorGroup = aggregatorGroup;
                        aggregatorUser.UserId = user.Id;
                        await _accountContext.AggregatorUsers.AddAsync(aggregatorUser);

                        RegisterFileRepositaryEF registerModel = null;

                        if (string.IsNullOrEmpty(model.RegisterFilename) == false)
                        {
                            registerModel = RegisterFile(user.Id, model.RegisterFilename, model.RegisterFilebase64);
                            _accountContext.RegisterFileRepositaries.Add(registerModel);
                        }

                        await _accountContext.SaveChangesAsync();

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
                            target = "중개거래사업자";

                            var agg_result = _accountContext.VwAggregatorusers.Where(x => x.AggGroupId == model.AggregatorGroupId);
                            supervisor_emails.AddRange(agg_result.Select(x => x.Email));

                            //var aggregator_account_users = await _userManager.GetUsersInRoleAsync(UserRoleTypes.Supervisor);
                            await _emailSender.SendEmailAsync(sender, $"새로운 {target} 가입이 요청되었습니다", email_contents, registerModel, supervisor_emails.ToArray());
                            logger.LogInformation($"가입 알림 메일 전송: {string.Join(", ", supervisor_emails)}");
                        }
                        trans.Commit();
                        return Ok(new { Result = result });
                    }
                    else
                    {
                        trans.Dispose();
                        return BadRequest(new { Result = result });
                    }
                }
                catch (Exception ex)
                {
                    trans.Dispose();
                    logger.LogError(ex, ex.Message);
                }
            }
            return BadRequest();
        }

        [HttpPost, Route("signonaccountmanager")]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SignonAccountManager([FromBody] BasicRegistModel model)
        {
            if (ModelState.IsValid)
            {
                try

                {
                    var user = CreateBasicAccount(model, RegisterType.AccountManager);
                    var result = await _userManager.CreateAsync(user, model.Password);
                    //result.Errors
                    if (result.Succeeded)
                    {
                        var role_add_result = await _userManager.AddToRoleAsync(user, UserRoleTypes.AccountManager);
                        ////_userManager.AddClaimAsync(user, new Claim())
                        //SupervisorUserEF aggregatorUser = new SupervisorUserEF();
                        //aggregatorUser.UserId = user.Id;
                        //await _accountContext.SupervisorUsers.AddAsync(aggregatorUser);
                        //await _accountContext.SaveChangesAsync();
                        return Ok(new { Result = role_add_result });
                    }
                    else
                    {
                        return BadRequest(new { Result = result });
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest();
        }

        [HttpPost, Route("signonhubbub")]
        [Authorize(Policy = UserPolicyTypes.AccountManager)]
        public async Task<IActionResult> SignonHubbub([FromBody] BasicRegistModel model)
        {
            if (ModelState.IsValid)
            {
                try

                {
                    var user = CreateBasicAccount(model, RegisterType.Hubbub);
                    var result = await _userManager.CreateAsync(user, model.Password);
                    //result.Errors
                    if (result.Succeeded)
                    {
                        var role_add_result = await _userManager.AddToRoleAsync(user, UserRoleTypes.HubbubUser);
                        return Ok(new { Result = role_add_result });
                    }
                    else
                    {
                        return BadRequest(new { Result = result });
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest();
        }


        [HttpPost, Route("signonsupervisor")]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SignonSupervisor([FromBody] BasicRegistModel model)
        {
            if (ModelState.IsValid)
            {
                var trans = await _accountContext.Database.BeginTransactionAsync();
                try

                {
                    var user = CreateSupervisorAccount(model);
                    var result = await _userManager.CreateAsync(user, model.Password);
                    //result.Errors
                    if (result.Succeeded)
                    {
                        var role_add_result = await _userManager.AddToRoleAsync(user, UserRoleTypes.Supervisor);
                        //_userManager.AddClaimAsync(user, new Claim())
                        SupervisorUserEF aggregatorUser = new SupervisorUserEF();
                        aggregatorUser.UserId = user.Id;
                        await _accountContext.SupervisorUsers.AddAsync(aggregatorUser);
                        await _accountContext.SaveChangesAsync();
                        trans.Commit();
                        return Ok(new { Result = result });
                    }
                    else
                    {
                        trans.Rollback();
                        return BadRequest(new { Result = result });
                    }
                }
                catch (Exception ex)
                {
                    trans.Dispose();
                    logger.LogError(ex, ex.Message);
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest();
        }

        [HttpPost, Route("signoncontractor")]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> SignonContractor([FromBody] AggregatorRegistModelBase model)
        {

            if (ModelState.IsValid)
            {
                var trans = await _accountContext.Database.BeginTransactionAsync();
                try
                {
                    AggregatorGroupEF aggregatorGroup = await _accountContext.AggregatorGroups.FindAsync(model.AggregatorGroupId);
                    if (aggregatorGroup == null)
                    {
                        IdentityError error = (_userManager.ErrorDescriber as LocalizedIdentityErrorDescriber).AggregatorNotFounded(model.AggregatorGroupId);
                        IdentityResult _result = IdentityResult.Failed(error);
                        return base.BadRequest(new { Result = _result });
                    }

                    var user = CreateUserAccount(model, RegisterType.Contrator);

                    JObject obj = JObject.FromObject(user);
                    var result = await _userManager.CreateAsync(user, model.Password);
                    //result.Errors
                    if (result.Succeeded)
                    {
                        ContractorUserEF cu = new ContractorUserEF();
                        cu.AggGroupId = aggregatorGroup.ID;
                        cu.ContractStatus = ContractStatusCodes.Signing;
                        cu.UserId = user.Id;
                        _accountContext.ContractorUsers.Add(cu);

                        RegisterFileRepositaryEF registerModel = null;

                        if (string.IsNullOrEmpty(model.RegisterFilename) == false)
                        {
                            registerModel = RegisterFile(user.Id, model.RegisterFilename, model.RegisterFilebase64);
                            _accountContext.RegisterFileRepositaries.Add(registerModel);
                        }

                        await _userManager.AddToRoleAsync(user, UserRoleTypes.Contractor);
                        await _accountContext.SaveChangesAsync();

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
                            target = "발전사업자";

                            var agg_result = _accountContext.VwAggregatorusers.Where(x => x.AggGroupId == model.AggregatorGroupId);
                            supervisor_emails.AddRange(agg_result.Select(x => x.Email));

                            //var aggregator_account_users = await _userManager.GetUsersInRoleAsync(UserRoleTypes.Supervisor);
                            await _emailSender.SendEmailAsync(sender, $"새로운 {target} 가입이 요청되었습니다", email_contents, registerModel, supervisor_emails.ToArray());
                            logger.LogInformation($"가입 알림 메일 전송: {string.Join(", ", supervisor_emails)}");
                        }
                        trans.Commit();
                        return Ok(new { Result = result });


                        //_userManager.find
                        //if (user.AuthRoles == (int)AuthRoles.Aggregator || user.AuthRoles == (int)AuthRoles.Business)
                        //    await Publisher.PublishMessageAsync(obj.ToString(), cancellationTokenSource.Token);

                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                        // Send an email with this link
                        //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                        //await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                        //    "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
                        //await _signInManager.SignInAsync(user, isPersistent: false);
                        //_logger.LogInformation(3, "User created a new account with password.");
                    }
                    else
                    {
                        trans.Dispose();
                        return BadRequest(new { Result = result });
                    }
                }
                catch (Exception ex)
                {
                    trans.Dispose();
                    logger.LogError(ex, ex.Message);
                    throw;
                }

            }

            // If we got this far, something failed, redisplay form
            return BadRequest();
        }

        private RegisterFileRepositaryEF RegisterFile(string userId, string fileName, string FileBase64Data)
        {
            if (string.IsNullOrEmpty(FileBase64Data))
                return null;
            RegisterFileRepositaryEF registerFile = new RegisterFileRepositaryEF();
            registerFile.UserId = userId;
            registerFile.FileName = fileName;
            string[] splits = FileBase64Data.Split(',');
            if (splits.Length > 1)
            {
                registerFile.ContentsType = splits[0];
                byte[] data = Convert.FromBase64String(splits[1]);
                registerFile.Contents = data;
            }
            else
            {
                registerFile.ContentsType = System.IO.Path.GetExtension(fileName).TrimStart('.');
                byte[] data = Convert.FromBase64String(splits[0]);
                registerFile.Contents = data;
            }
            registerFile.CreateDT = DateTime.Now.Date;
            return registerFile;
        }
    }
}