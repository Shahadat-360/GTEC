using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using MiniAccountManagementSystem.Web.Services;


namespace MiniAccountManagementSystem.Web.Pages.ManageModules
{
    [Authorize(Roles = "Admin")]
    public class AssignModuleModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AssignModuleModel(IConfiguration configuration, RoleManager<IdentityRole> roleManager)
    {
        _configuration = configuration;
        _roleManager = roleManager;
    }

    [BindProperty]
    public string SelectedRole { get; set; }

    [BindProperty]
    public string SelectedModule { get; set; }

    public List<string> Roles { get; set; } = new();
    public List<string> Modules { get; set; } = new();
    public string Message { get; set; }

    public async Task OnGetAsync()
    {
        Roles = _roleManager.Roles.Select(r => r.Name).ToList();

        using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();
        using var cmd = new SqlCommand("SELECT ModuleName FROM Modules", connection);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Modules.Add(reader.GetString(0));
        }
    }

    public async Task<IActionResult> OnPostAssignAsync()
    {
        await RunStoredProcedure("sp_AssignModuleToRole");
        Message = "Module assigned successfully.";
        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostRemoveAsync()
    {
        await RunStoredProcedure("sp_RemoveModuleFromRole");
        Message = "Module removed successfully.";
        await OnGetAsync();
        return Page();
    }

    private async Task RunStoredProcedure(string procedureName)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();

        using var cmd = new SqlCommand(procedureName, connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@RoleName", SelectedRole);
        cmd.Parameters.AddWithValue("@ModuleName", SelectedModule);
        await cmd.ExecuteNonQueryAsync();
    }
    }
}
