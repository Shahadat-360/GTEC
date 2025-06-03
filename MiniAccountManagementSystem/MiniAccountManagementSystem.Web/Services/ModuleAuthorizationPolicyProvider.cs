using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace MiniAccountManagementSystem.Web.Services
{
    public class ModuleAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

        public ModuleAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(ModuleAuthorizeAttribute.POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                var moduleName = policyName.Substring(ModuleAuthorizeAttribute.POLICY_PREFIX.Length + 1);

                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new ModuleAccessRequirement(moduleName))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }

            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}