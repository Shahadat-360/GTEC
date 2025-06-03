using Microsoft.AspNetCore.Authorization;

namespace MiniAccountManagementSystem.Web.Services
{
    public class ModuleAuthorizeAttribute : AuthorizeAttribute
    {
        public const string POLICY_PREFIX = "ModuleAccess";

        public ModuleAuthorizeAttribute(string moduleName)
        {
            ModuleName = moduleName;
            Policy = $"{POLICY_PREFIX}:{moduleName}";
        }

        public string ModuleName { get; }
    }
}