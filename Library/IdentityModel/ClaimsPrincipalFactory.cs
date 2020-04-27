using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PeiuPlatform.Model.Database;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PeiuPlatform.Model
{
    public class ClaimsPrincipalFactory : UserClaimsPrincipalFactory<UserAccountEF, Role>
    {
        readonly ILogger<AuthorizationPolicyProvider> _logger;
        public ClaimsPrincipalFactory(
            UserManager<UserAccountEF> userManager,
            RoleManager<Role> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<AuthorizationPolicyProvider> logger
            )
            : base(userManager, roleManager, optionsAccessor)
        {
            _logger = logger;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(UserAccountEF user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            try
            {
                identity.AddClaim(new Claim("CompanyName", user.CompanyName ?? ""));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return identity;
        }
    }
}
