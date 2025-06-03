using System.ComponentModel.DataAnnotations;

namespace MiniAccountManagementSystem.Web.Entities
{
    public class ChartOfAccountViewModel
    {
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Account Code is required")]
        [StringLength(20, ErrorMessage = "Account Code cannot exceed 20 characters")]
        [Display(Name = "Account Code")]
        public string AccountCode { get; set; }

        [Required(ErrorMessage = "Account Name is required")]
        [StringLength(100, ErrorMessage = "Account Name cannot exceed 100 characters")]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        [Display(Name = "Parent Account")]
        public int? ParentAccountId { get; set; }

        [Display(Name = "Parent Account")]
        public string ParentAccountName { get; set; }

        [Required(ErrorMessage = "Account Type is required")]
        [Display(Name = "Account Type")]
        public AccountType AccountType { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Last Modified")]
        public DateTime? LastModifiedDate { get; set; }

        public int ChildCount { get; set; }

        public string AccountTypeName => AccountType.ToString();

        public string StatusName => IsActive ? "Active" : "Inactive";
    }

    public class ChartOfAccountListViewModel
    {
        public List<ChartOfAccountViewModel> Accounts { get; set; } = new List<ChartOfAccountViewModel>();
        
        public AccountType? SelectedAccountType { get; set; }
        
        public bool? SelectedIsActive { get; set; }
        
        public int? SelectedParentAccountId { get; set; }
        
        public List<ChartOfAccountViewModel> ParentAccounts { get; set; } = new List<ChartOfAccountViewModel>();
        
        public string SearchTerm { get; set; }
        
        public ChartOfAccountStatistics Statistics { get; set; } = new ChartOfAccountStatistics();
    }

    public class ChartOfAccountStatistics
    {
        public int TotalAccounts { get; set; }
        public int AssetAccounts { get; set; }
        public int LiabilityAccounts { get; set; }
        public int EquityAccounts { get; set; }
        public int RevenueAccounts { get; set; }
        public int ExpenseAccounts { get; set; }
        public int ActiveAccounts { get; set; }
        public int InactiveAccounts { get; set; }
    }
}