using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniAccountManagementSystem.Web.Entities
{
    public class RoleModule
    {
        [Key]
        public int RoleModuleId { get; set; }

        [Required]
        public string RoleId { get; set; }

        [Required]
        public int ModuleId { get; set; }

        [ForeignKey("RoleId")]
        public IdentityRole Role { get; set; }

        [ForeignKey("ModuleId")]
        public Module Module { get; set; }
    }

}
