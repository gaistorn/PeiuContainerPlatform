using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PeiuPlatform.Model.IdentityModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PeiuPlatform.Model
{
    public class AuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly AuthorizationOptions _options;
        readonly ILogger<AuthorizationPolicyProvider> _logger;

        public AuthorizationPolicyProvider(ILogger<AuthorizationPolicyProvider> logger, IOptions<AuthorizationOptions> options) : base(options)
        {
            _options = options.Value;
            _logger = logger;
        }

        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // Check static policies first
            var policy = await base.GetPolicyAsync(policyName);

            if (policy == null)
            {
                policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new HasScopeRequirement(policyName, UserClaimTypes.Issuer))
                    .Build();

                // Add policy to the AuthorizationOptions, so we don't have to re-create it each time
                _options.AddPolicy(policyName, policy);
            }

            return policy;
        }
    }
}