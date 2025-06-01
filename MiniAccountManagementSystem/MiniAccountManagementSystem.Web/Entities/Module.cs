using System.ComponentModel.DataAnnotations;

namespace MiniAccountManagementSystem.Web.Entities
{
    public class Module
    {
        [Key]
        public int ModuleId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ModuleName { get; set; }

        public ICollection<RoleModule> RoleModules { get; set; }
    }
}
