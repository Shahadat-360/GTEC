using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniAccountManagementSystem.Web.Entities
{
    public class ChartOfAccount
    {
        [Key]
        public int AccountId { get; set; }

        [Required]
        [MaxLength(20)]
        public string AccountCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string AccountName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public int? ParentAccountId { get; set; }

        [ForeignKey("ParentAccountId")]
        public ChartOfAccount ParentAccount { get; set; }

        public ICollection<ChartOfAccount> ChildAccounts { get; set; }

        [Required]
        public AccountType AccountType { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastModifiedDate { get; set; }
    }

    public enum AccountType
    {
        Asset = 1,
        Liability = 2,
        Equity = 3,
        Revenue = 4,
        Expense = 5
    }
}