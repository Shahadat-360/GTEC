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
    public class DetailsModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public DetailsModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ChartOfAccountViewModel ChartOfAccount { get; set; } = new ChartOfAccountViewModel();
        public List<ChartOfAccountViewModel> ChildAccounts { get; set; } = new List<ChartOfAccountViewModel>();

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
            await LoadChildAccountsAsync(id.Value);

            return Page();
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

        private async Task LoadChildAccountsAsync(int parentId)
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_GetChartOfAccounts", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ParentAccountId", parentId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ChildAccounts.Add(new ChartOfAccountViewModel
                                {
                                    AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                                    AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                                    AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    AccountType = (AccountType)reader.GetInt32(reader.GetOrdinal("AccountType")),
                                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    ChildCount = reader.GetInt32(reader.GetOrdinal("ChildCount"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error loading child accounts: {ex.Message}");
            }
        }
    }
}