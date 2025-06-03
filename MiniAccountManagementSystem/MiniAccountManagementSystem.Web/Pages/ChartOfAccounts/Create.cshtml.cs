using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Web.Entities;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using MiniAccountManagementSystem.Web.Services;

namespace MiniAccountManagementSystem.Web.Pages.ChartOfAccounts
{
    [ModuleAuthorize("ChartOfAccounts")]
    public class CreateModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public CreateModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public ChartOfAccountViewModel ChartOfAccount { get; set; } = new ChartOfAccountViewModel();

        public SelectList AccountTypeSelectList { get; set; }
        public SelectList ParentAccountSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDropdownDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Remove ParentAccountName from ModelState validation as it's not a form field that needs validation
            ModelState.Remove("ChartOfAccount.ParentAccountName");

            if (!ModelState.IsValid)
            {
                await LoadDropdownDataAsync();
                return Page();
            }

            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_CreateChartOfAccount", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters
                        command.Parameters.AddWithValue("@AccountCode", ChartOfAccount.AccountCode);
                        command.Parameters.AddWithValue("@AccountName", ChartOfAccount.AccountName);
                        command.Parameters.AddWithValue("@Description", (object)ChartOfAccount.Description ?? DBNull.Value);
                        
                        // Fix for decimal to int conversion issue
                        if (ChartOfAccount.ParentAccountId.HasValue)
                        {
                            // Convert decimal to int if needed
                            int parentId = Convert.ToInt32(ChartOfAccount.ParentAccountId.Value);
                            command.Parameters.AddWithValue("@ParentAccountId", parentId == 0 ? DBNull.Value : (object)parentId);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@ParentAccountId", DBNull.Value);
                        }
                        
                        command.Parameters.AddWithValue("@AccountType", (int)ChartOfAccount.AccountType);
                        command.Parameters.AddWithValue("@IsActive", ChartOfAccount.IsActive);
                        try
                        {
                            // Execute the stored procedure and get the new account ID
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    var accountId = Convert.ToInt32(reader["AccountId"]);
                                    TempData["SuccessMessage"] = $"Account '{ChartOfAccount.AccountName}' created successfully.";
                                    return RedirectToPage("./Index");
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            // Handle specific SQL errors
                            if (ex.Message.Contains("Account code already exists"))
                            {
                                ModelState.AddModelError("ChartOfAccount.AccountCode", "This account code already exists. Please use a different code.");
                            }
                            else if (ex.Message.Contains("Parent account does not exist"))
                            {
                                ModelState.AddModelError("ChartOfAccount.ParentAccountId", "The selected parent account does not exist.");
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, $"Database error: {ex.Message}");
                            }

                            await LoadDropdownDataAsync();
                            return Page();
                        }
                    }
                }

                // If we get here, something unexpected happened
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while creating the account.");
                await LoadDropdownDataAsync();
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                await LoadDropdownDataAsync();
                return Page();
            }
        }

        private async Task LoadDropdownDataAsync()
        {
            // Load account types for dropdown
            var accountTypes = Enum.GetValues(typeof(AccountType))
                .Cast<AccountType>()
                .Select(a => new { Id = (int)a, Name = a.ToString() })
                .ToList();

            AccountTypeSelectList = new SelectList(accountTypes, "Id", "Name");

            // Load parent accounts for dropdown
            var parentAccounts = new List<object>();

            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_GetChartOfAccounts", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                parentAccounts.Add(new
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("AccountId")),
                                    Name = $"{reader.GetString(reader.GetOrdinal("AccountCode"))} - {reader.GetString(reader.GetOrdinal("AccountName"))}"
                                });
                            }
                        }
                    }
                }

                // Create the SelectList with or without items
                ParentAccountSelectList = new SelectList(parentAccounts, "Id", "Name");
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error loading parent accounts: {ex.Message}");
                // Create an empty select list to avoid null reference exceptions
                ParentAccountSelectList = new SelectList(new List<object>());
            }
        }
    }
}