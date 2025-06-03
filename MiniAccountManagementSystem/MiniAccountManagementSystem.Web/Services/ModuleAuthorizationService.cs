using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace MiniAccountManagementSystem.Web.Services
{
    public interface IModuleAuthorizationService
    {
        Task<bool> UserHasModuleAccessAsync(ClaimsPrincipal user, string moduleName);
    }

    public class ModuleAuthorizationService : IModuleAuthorizationService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public ModuleAuthorizationService(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<bool> UserHasModuleAccessAsync(ClaimsPrincipal user, string moduleName)
        {
            if (!user.Identity.IsAuthenticated)
            {
                return false;
            }

            // Admin role always has access to all modules
            if (user.IsInRole("Admin"))
            {
                return true;
            }

            var userId = _userManager.GetUserId(user);
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var identityUser = await _userManager.FindByIdAsync(userId);
            if (identityUser == null)
            {
                return false;
            }

            // Get user roles
            var userRoles = await _userManager.GetRolesAsync(identityUser);
            if (userRoles == null || !userRoles.Any())
            {
                return false;
            }

            // Check if any of the user's roles have access to the module
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // Build a query with a dynamic IN clause for roles
                var sqlQuery = "SELECT COUNT(1) FROM RoleModules rm " +
                               "INNER JOIN AspNetRoles r ON rm.RoleId = r.Id " +
                               "INNER JOIN Modules m ON rm.ModuleId = m.ModuleId " +
                               "WHERE m.ModuleName = @ModuleName AND r.Name IN (";

                // Add parameters for each role
                for (int i = 0; i < userRoles.Count; i++)
                {
                    var paramName = $"@Role{i}";
                    sqlQuery += (i > 0 ? ", " : "") + paramName;
                }
                sqlQuery += ")";

                using (var command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@ModuleName", moduleName);
                    
                    // Add each role as a separate parameter
                    for (int i = 0; i < userRoles.Count; i++)
                    {
                        command.Parameters.AddWithValue($"@Role{i}", userRoles[i]);
                    }

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result) > 0;
                }
            }
        }
    }
}