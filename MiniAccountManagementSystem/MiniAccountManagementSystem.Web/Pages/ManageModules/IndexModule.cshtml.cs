using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MiniAccountManagementSystem.Web.Data;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using MiniAccountManagementSystem.Web.Services;

namespace MiniAccountManagementSystem.Web.Pages.ManageModules
{
    [Authorize(Roles = "Admin")]
    public class IndexModuleModel(ApplicationDbContext context) : PageModel
    {
        private readonly ApplicationDbContext _context = context;

        // Properties for the view
        public int TotalRoles { get; set; }
        public int TotalModules { get; set; }
        public int TotalAssignments { get; set; }
        public int RolesWithModules { get; set; }

        public List<string> AllRoles { get; set; } = new();
        public List<string> AllModules { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string SelectedRoleFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedModuleFilter { get; set; }
        public List<RoleModuleAssignment> RoleModuleAssignments { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadStatisticsAsync();
            await LoadDropdownDataAsync();
            await LoadAssignmentsAsync();
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "sp_GetModuleAssignmentStatistics";
                command.CommandType = CommandType.StoredProcedure;

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    TotalRoles = reader.GetInt32("TotalRoles");
                    TotalModules = reader.GetInt32("TotalModules");
                    TotalAssignments = reader.GetInt32("TotalAssignments");
                    RolesWithModules = reader.GetInt32("RolesWithModules");
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error loading statistics: {ex.Message}");
            }
        }
        private async Task LoadDropdownDataAsync()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                // Load roles
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_GetAllRoles";
                    command.CommandType = CommandType.StoredProcedure;

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        AllRoles.Add(reader.GetString("Name"));
                    }
                }

                // Load modules
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_GetAllModules";
                    command.CommandType = CommandType.StoredProcedure;

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        AllModules.Add(reader.GetString("ModuleName"));
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error loading dropdown data: {ex.Message}");
            }
        }
        private async Task LoadAssignmentsAsync()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();

                // Use filtered or all assignments based on filters
                if (!string.IsNullOrEmpty(SelectedRoleFilter) || !string.IsNullOrEmpty(SelectedModuleFilter))
                {
                    command.CommandText = "sp_GetFilteredRoleModuleAssignments";
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    var roleParam = new SqlParameter("@RoleName", SqlDbType.NVarChar, 256)
                    {
                        Value = string.IsNullOrEmpty(SelectedRoleFilter) ? DBNull.Value : SelectedRoleFilter
                    };
                    var moduleParam = new SqlParameter("@ModuleName", SqlDbType.NVarChar, 100)
                    {
                        Value = string.IsNullOrEmpty(SelectedModuleFilter) ? DBNull.Value : SelectedModuleFilter
                    };

                    command.Parameters.Add(roleParam);
                    command.Parameters.Add(moduleParam);
                }
                else
                {
                    command.CommandText = "sp_GetAllRoleModuleAssignments";
                    command.CommandType = CommandType.StoredProcedure;
                }

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    RoleModuleAssignments.Add(new RoleModuleAssignment
                    {
                        RoleName = reader.GetString("RoleName"),
                        ModuleName = reader.GetString("ModuleName"),
                        AssignedDate = reader.IsDBNull("AssignedDate") ? null : reader.GetDateTime("AssignedDate")
                    });
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error loading assignments: {ex.Message}");
            }
        }
    }
    // Models
    public class RoleModuleAssignment
    {
        public string RoleName { get; set; }
        public string ModuleName { get; set; }
        public DateTime? AssignedDate { get; set; }
    }

    public class ModuleAssignmentStatistics
    {
        public int TotalRoles { get; set; }
        public int TotalModules { get; set; }
        public int TotalAssignments { get; set; }
        public int RolesWithModules { get; set; }
    }
}
 