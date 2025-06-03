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
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty(SupportsGet = true)]
        public ChartOfAccountListViewModel ChartOfAccountList { get; set; } = new ChartOfAccountListViewModel();

        public SelectList AccountTypeSelectList { get; set; }
        public SelectList IsActiveSelectList { get; set; }
        public SelectList ParentAccountSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadFilterDataAsync();
            await LoadChartOfAccountsAsync();
            await LoadStatisticsAsync();
            return Page();
        }

        private async Task LoadFilterDataAsync()
        {
            // Load account types for filter dropdown
            var accountTypes = Enum.GetValues(typeof(AccountType))
                .Cast<AccountType>()
                .Select(a => new { Id = (int)a, Name = a.ToString() })
                .ToList();

            AccountTypeSelectList = new SelectList(accountTypes, "Id", "Name");

            // Load active/inactive options for filter dropdown
            var activeOptions = new List<object>
            {
                new { Id = true, Name = "Active" },
                new { Id = false, Name = "Inactive" }
            };

            IsActiveSelectList = new SelectList(activeOptions, "Id", "Name");

            // Load parent accounts for filter dropdown
            ChartOfAccountList.ParentAccounts = await GetParentAccountsAsync();

            var parentAccounts = ChartOfAccountList.ParentAccounts
                .Select(a => new { Id = a.AccountId, Name = $"{a.AccountCode} - {a.AccountName}" })
                .ToList();

            // Add "Root Accounts" option
            parentAccounts.Insert(0, new { Id = 0, Name = "Root Accounts (No Parent)" });

            ParentAccountSelectList = new SelectList(parentAccounts, "Id", "Name");
        }

        private async Task<List<ChartOfAccountViewModel>> GetParentAccountsAsync()
        {
            var accounts = new List<ChartOfAccountViewModel>();

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
                                accounts.Add(new ChartOfAccountViewModel
                                {
                                    AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                                    AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                                    AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                                    AccountType = (AccountType)reader.GetInt32(reader.GetOrdinal("AccountType")),
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
                Console.WriteLine($"Error loading parent accounts: {ex.Message}");
            }

            return accounts;
        }

        private async Task LoadChartOfAccountsAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_GetChartOfAccounts", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters for filtering
                        if (ChartOfAccountList.SelectedAccountType.HasValue)
                        {
                            command.Parameters.AddWithValue("@AccountType", (int)ChartOfAccountList.SelectedAccountType.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@AccountType", DBNull.Value);
                        }

                        if (ChartOfAccountList.SelectedIsActive.HasValue)
                        {
                            command.Parameters.AddWithValue("@IsActive", ChartOfAccountList.SelectedIsActive.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@IsActive", DBNull.Value);
                        }

                        if (ChartOfAccountList.SelectedParentAccountId.HasValue)
                        {
                            command.Parameters.AddWithValue("@ParentAccountId", ChartOfAccountList.SelectedParentAccountId.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@ParentAccountId", DBNull.Value);
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            ChartOfAccountList.Accounts = new List<ChartOfAccountViewModel>();

                            while (await reader.ReadAsync())
                            {
                                var account = new ChartOfAccountViewModel
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

                                // Apply search filter if provided
                                if (string.IsNullOrWhiteSpace(ChartOfAccountList.SearchTerm) ||
                                    account.AccountCode.Contains(ChartOfAccountList.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                    account.AccountName.Contains(ChartOfAccountList.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                    (account.Description != null && account.Description.Contains(ChartOfAccountList.SearchTerm, StringComparison.OrdinalIgnoreCase)))
                                {
                                    ChartOfAccountList.Accounts.Add(account);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error loading chart of accounts: {ex.Message}");
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    // Get total accounts count
                    using (var command = new SqlCommand("SELECT COUNT(*) FROM ChartOfAccounts", connection))
                    {
                        ChartOfAccountList.Statistics.TotalAccounts = (int)await command.ExecuteScalarAsync();
                    }

                    // Get account type counts
                    using (var command = new SqlCommand("SELECT AccountType, COUNT(*) AS Count FROM ChartOfAccounts GROUP BY AccountType", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var accountType = (AccountType)reader.GetInt32(0);
                                var count = reader.GetInt32(1);

                                switch (accountType)
                                {
                                    case AccountType.Asset:
                                        ChartOfAccountList.Statistics.AssetAccounts = count;
                                        break;
                                    case AccountType.Liability:
                                        ChartOfAccountList.Statistics.LiabilityAccounts = count;
                                        break;
                                    case AccountType.Equity:
                                        ChartOfAccountList.Statistics.EquityAccounts = count;
                                        break;
                                    case AccountType.Revenue:
                                        ChartOfAccountList.Statistics.RevenueAccounts = count;
                                        break;
                                    case AccountType.Expense:
                                        ChartOfAccountList.Statistics.ExpenseAccounts = count;
                                        break;
                                }
                            }
                        }
                    }

                    // Get active/inactive counts
                    using (var command = new SqlCommand("SELECT IsActive, COUNT(*) AS Count FROM ChartOfAccounts GROUP BY IsActive", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var isActive = reader.GetBoolean(0);
                                var count = reader.GetInt32(1);

                                if (isActive)
                                {
                                    ChartOfAccountList.Statistics.ActiveAccounts = count;
                                }
                                else
                                {
                                    ChartOfAccountList.Statistics.InactiveAccounts = count;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error loading statistics: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostFilterAsync()
        {
            await LoadFilterDataAsync();
            await LoadChartOfAccountsAsync();
            await LoadStatisticsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostClearFiltersAsync()
        {
            ChartOfAccountList.SelectedAccountType = null;
            ChartOfAccountList.SelectedIsActive = null;
            ChartOfAccountList.SelectedParentAccountId = null;
            ChartOfAccountList.SearchTerm = null;

            await LoadFilterDataAsync();
            await LoadChartOfAccountsAsync();
            await LoadStatisticsAsync();
            return Page();
        }
    }
}