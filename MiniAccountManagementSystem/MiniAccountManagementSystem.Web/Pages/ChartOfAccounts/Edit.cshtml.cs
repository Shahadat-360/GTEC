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
    public class EditModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public EditModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public ChartOfAccountViewModel ChartOfAccount { get; set; } = new ChartOfAccountViewModel();

        public SelectList AccountTypeSelectList { get; set; }
        public SelectList ParentAccountSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await GetAccountByIdAsync(id.Value);
            if (account == null)
            {
                return NotFound();
            }

            ChartOfAccount = account;
            await LoadDropdownDataAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Debug information to check what's coming from the form
            Console.WriteLine($"ParentAccountId from form: {ChartOfAccount.ParentAccountId}");
            
            // Handle empty string from dropdown as null
            if (ChartOfAccount.ParentAccountId.HasValue && ChartOfAccount.ParentAccountId.Value == 0)
            {
                ChartOfAccount.ParentAccountId = null;
            }

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

                    using (var command = new SqlCommand("sp_UpdateChartOfAccount", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters
                        command.Parameters.AddWithValue("@AccountId", ChartOfAccount.AccountId);
                        command.Parameters.AddWithValue("@AccountCode", ChartOfAccount.AccountCode);
                        command.Parameters.AddWithValue("@AccountName", ChartOfAccount.AccountName);
                        command.Parameters.AddWithValue("@Description", (object)ChartOfAccount.Description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ParentAccountId", (object)ChartOfAccount.ParentAccountId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@AccountType", (int)ChartOfAccount.AccountType);
                        command.Parameters.AddWithValue("@IsActive", ChartOfAccount.IsActive);

                        try
                        {
                            // Execute the stored procedure
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    TempData["SuccessMessage"] = $"Account '{ChartOfAccount.AccountName}' updated successfully.";
                                    return RedirectToPage("./Index");
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            // Handle specific SQL errors
                            if (ex.Message.Contains("Account code already exists"))
                            {
                                ModelState.AddModelError("ChartOfAccount.AccountCode", "This account code already exists for another account. Please use a different code.");
                            }
                            else if (ex.Message.Contains("Parent account does not exist"))
                            {
                                ModelState.AddModelError("ChartOfAccount.ParentAccountId", "The selected parent account does not exist.");
                            }
                            else if (ex.Message.Contains("Cannot set account as its own parent"))
                            {
                                ModelState.AddModelError("ChartOfAccount.ParentAccountId", "An account cannot be its own parent.");
                            }
                            else if (ex.Message.Contains("Account does not exist"))
                            {
                                return NotFound();
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
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the account.");
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

        private async Task<ChartOfAccountViewModel> GetAccountByIdAsync(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_GetChartOfAccountById", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@AccountId", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new ChartOfAccountViewModel
                                {
                                    AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                                    AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                                    AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    ParentAccountId = reader.IsDBNull(reader.GetOrdinal("ParentAccountId")) ? null : reader.GetInt32(reader.GetOrdinal("ParentAccountId")),
                                    ParentAccountName = reader.IsDBNull(reader.GetOrdinal("ParentAccountName")) ? null : reader.GetString(reader.GetOrdinal("ParentAccountName")),
                                    AccountType = (AccountType)reader.GetInt32(reader.GetOrdinal("AccountType")),
                                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    LastModifiedDate = reader.IsDBNull(reader.GetOrdinal("LastModifiedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("LastModifiedDate"))
                                };
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error getting account by ID: {ex.Message}");
                return null;
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
                                // Skip the current account to prevent circular references
                                if (reader.GetInt32(reader.GetOrdinal("AccountId")) == ChartOfAccount.AccountId)
                                    continue;

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