using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MiniAccountManagementSystem.Web.Entities
{
    public class UserViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Roles")]
        public string Roles { get; set; }

        [Display(Name = "Account Locked")]
        public bool LockoutEnabled { get; set; }

        [Display(Name = "Failed Attempts")]
        public int AccessFailedCount { get; set; }
    }

    public class UserRoleViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Assigned Roles")]
        public List<string> AssignedRoles { get; set; } = new List<string>();

        [Display(Name = "Available Roles")]
        public List<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();

        [Display(Name = "Selected Roles")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }

    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please select at least one role")]
        [Display(Name = "Roles")]
        public List<string> SelectedRoles { get; set; } = new List<string>();

        [Display(Name = "Available Roles")]
        public List<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();
    }

    public class UserListViewModel
    {
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();

        public string SelectedRoleId { get; set; }

        public List<IdentityRole> AvailableRoles { get; set; } = new List<IdentityRole>();

        public string SearchTerm { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalRecords { get; set; }

        public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;
    }
}