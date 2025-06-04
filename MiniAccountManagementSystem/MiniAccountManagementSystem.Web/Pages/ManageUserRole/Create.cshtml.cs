using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniAccountManagementSystem.Web.Entities;

namespace MiniAccountManagementSystem.Web.Pages.ManageUserRole
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CreateModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public CreateUserViewModel UserModel { get; set; } = new CreateUserViewModel();

        public IActionResult OnGet()
        {
            // Load available roles
            LoadAvailableRoles();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Reload available roles in case of validation errors
            LoadAvailableRoles();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if at least one role is selected
            if (UserModel.SelectedRoles == null || !UserModel.SelectedRoles.Any())
            {
                ModelState.AddModelError("UserModel.SelectedRoles", "Please select at least one role.");
                return Page();
            }

            // Create the user
            var user = new IdentityUser
            {
                UserName = UserModel.Email,
                Email = UserModel.Email,
                EmailConfirmed = true // Auto-confirm email for admin-created accounts
            };

            var result = await _userManager.CreateAsync(user, UserModel.Password);

            if (result.Succeeded)
            {
                // Assign selected roles to the user
                foreach (var roleName in UserModel.SelectedRoles)
                {
                    // Verify the role exists
                    if (await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _userManager.AddToRoleAsync(user, roleName);
                    }
                }

                TempData["SuccessMessage"] = $"User '{user.Email}' has been created successfully.";
                return RedirectToPage("./Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        private void LoadAvailableRoles()
        {
            UserModel.AvailableRoles = _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
                .ToList();
        }
    }
}