using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MiniAccountManagementSystem.Web.Entities;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using MiniAccountManagementSystem.Web.Services;

namespace MiniAccountManagementSystem.Web.Pages.ChartOfAccounts
{
    [ModuleAuthorize("ChartOfAccounts")]
    public class DeleteModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public DeleteModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public ChartOfAccountViewModel ChartOfAccount { get; set; } = new ChartOfAccountViewModel();

        public string ErrorMessage { get; set; }

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
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ChartOfAccount.AccountId == 0)
            {
                return NotFound();
            }

            try
            {
                var result = await DeleteAccountAsync(ChartOfAccount.AccountId);
                if (result.Success)
                {
                    return RedirectToPage("./Index");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage;
                    // Reload the account data
                    ChartOfAccount = await GetAccountByIdAsync(ChartOfAccount.AccountId);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while deleting the account: {ex.Message}";
                // Reload the account data
                ChartOfAccount = await GetAccountByIdAsync(ChartOfAccount.AccountId);
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
                                    LastModifiedDate = reader.IsDBNull(reader.GetOrdinal("LastModifiedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("LastModifiedDate")),
                                    ChildCount = reader.GetInt32(reader.GetOrdinal("ChildCount"))
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

        private async Task<(bool Success, string ErrorMessage)> DeleteAccountAsync(int accountId)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_DeleteChartOfAccount", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@AccountId", accountId);

                        // Add output parameter for error message
                        var errorMessageParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 500)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(errorMessageParam);

                        // Add return value parameter
                        var returnValue = new SqlParameter("@ReturnValue", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.ReturnValue
                        };
                        command.Parameters.Add(returnValue);

                        await command.ExecuteNonQueryAsync();

                        // Check the return value
                        int result = (int)returnValue.Value;
                        if (result == 0)
                        {
                            return (true, null);
                        }
                        else
                        {
                            string errorMessage = errorMessageParam.Value?.ToString();
                            return (false, string.IsNullOrEmpty(errorMessage) ? "Failed to delete the account." : errorMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }
    }
}