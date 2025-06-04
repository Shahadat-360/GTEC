using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Web.Entities;
using MiniAccountManagementSystem.Web.Services;
using System.Data;

namespace MiniAccountManagementSystem.Web.Pages.ManageUserRole
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(IConfiguration configuration, RoleManager<IdentityRole> roleManager)
        {
            _configuration = configuration;
            _roleManager = roleManager;
        }

        [BindProperty(SupportsGet = true)]
        public UserListViewModel UserList { get; set; } = new UserListViewModel();


        [BindProperty(SupportsGet = true)]
        public int pageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public string selectedRoleId { get; set; } = null;
        [BindProperty(SupportsGet = true)]
        public string searchTerm { get; set; } = null;

        public async Task<IActionResult> OnGetAsync()
        {
            // Set pagination parameters
            UserList.PageNumber = pageNumber < 1 ? 1 : pageNumber;
            UserList.SelectedRoleId = selectedRoleId;
            UserList.SearchTerm = searchTerm;

            // Load available roles for filter dropdown
            UserList.AvailableRoles = _roleManager.Roles.ToList();

            await LoadUsersAsync();

            return Page();
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_GetUsersWithRoles", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters for filtering and pagination
                        if (!string.IsNullOrEmpty(UserList.SelectedRoleId))
                        {
                            command.Parameters.AddWithValue("@RoleId", UserList.SelectedRoleId);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@RoleId", DBNull.Value);
                        }

                        if (!string.IsNullOrEmpty(UserList.SearchTerm))
                        {
                            command.Parameters.AddWithValue("@SearchTerm", UserList.SearchTerm);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@SearchTerm", DBNull.Value);
                        }

                        command.Parameters.AddWithValue("@PageNumber", UserList.PageNumber);
                        command.Parameters.AddWithValue("@PageSize", UserList.PageSize);

                        var totalRecordsParam = new SqlParameter("@TotalRecords", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(totalRecordsParam);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            UserList.Users = new List<UserViewModel>();

                            while (await reader.ReadAsync())
                            {
                                UserList.Users.Add(new UserViewModel
                                {
                                    Id = reader["UserId"].ToString(),
                                    UserName = reader["UserName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    EmailConfirmed = (bool)reader["EmailConfirmed"],
                                    PhoneNumber = reader["PhoneNumber"] != DBNull.Value ? reader["PhoneNumber"].ToString() : null,
                                    LockoutEnabled = (bool)reader["LockoutEnabled"],
                                    AccessFailedCount = (int)reader["AccessFailedCount"],
                                    Roles = reader["Roles"] != DBNull.Value ? reader["Roles"].ToString() : null
                                });
                            }
                        }

                        // Get total records count from output parameter
                        UserList.TotalRecords = (int)totalRecordsParam.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                ModelState.AddModelError(string.Empty, $"Error loading users: {ex.Message}");
            }
        }
    }
}