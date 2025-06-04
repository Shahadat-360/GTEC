using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniAccountManagementSystem.Web.Entities;
using System.Security.Claims;

namespace MiniAccountManagementSystem.Web.Pages.ManageUserRole
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EditModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public UserRoleViewModel UserRoles { get; set; } = new UserRoleViewModel();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Get current user roles
            var userRoles = await _userManager.GetRolesAsync(user);

            UserRoles = new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                AssignedRoles = userRoles.ToList()
            };

            // Load available roles
            LoadAvailableRoles();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LoadAvailableRoles();
                return Page();
            }

            var user = await _userManager.FindByIdAsync(UserRoles.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Get current user roles
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Ensure the selected roles list is not null
            UserRoles.SelectedRoles ??= new List<string>();

            // Prevent removing Admin role from the default admin account
            var adminEmail = "Admin@qtec.com";
            if (user.Email.Equals(adminEmail, StringComparison.OrdinalIgnoreCase) && 
                currentRoles.Contains("Admin") && 
                !UserRoles.SelectedRoles.Contains("Admin"))
            {
                ModelState.AddModelError(string.Empty, "Cannot remove Admin role from the default administrator account.");
                LoadAvailableRoles();
                return Page();
            }

            // Roles to remove (roles that were assigned but are not in the selected roles)
            var rolesToRemove = currentRoles.Where(r => !UserRoles.SelectedRoles.Contains(r)).ToList();

            // Roles to add (roles that are selected but were not assigned)
            var rolesToAdd = UserRoles.SelectedRoles.Where(r => !currentRoles.Contains(r)).ToList();

            // Remove roles
            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    foreach (var error in removeResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    LoadAvailableRoles();
                    return Page();
                }
            }

            // Add roles
            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    LoadAvailableRoles();
                    return Page();
                }
            }

            TempData["SuccessMessage"] = $"Roles for user '{user.Email}' have been updated successfully.";
            return RedirectToPage("./Index");
        }

        private void LoadAvailableRoles()
        {
            UserRoles.AvailableRoles = _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
                .ToList();
        }
    }
}