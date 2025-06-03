using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MiniAccountManagementSystem.Web.Services
{
    public class ModuleAccessRequirement : IAuthorizationRequirement
    {
        public string ModuleName { get; }

        public ModuleAccessRequirement(string moduleName)
        {
            ModuleName = moduleName;
        }
    }

    public class ModuleAuthorizationHandler : AuthorizationHandler<ModuleAccessRequirement>
    {
        private readonly IModuleAuthorizationService _moduleAuthorizationService;

        public ModuleAuthorizationHandler(IModuleAuthorizationService moduleAuthorizationService)
        {
            _moduleAuthorizationService = moduleAuthorizationService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ModuleAccessRequirement requirement)
        {
            if (context.User == null)
            {
                return;
            }

            if (await _moduleAuthorizationService.UserHasModuleAccessAsync(context.User, requirement.ModuleName))
            {
                context.Succeed(requirement);
            }
        }
    }
}