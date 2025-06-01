using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace MiniAccountManagementSystem.Web.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            string[] roles = { "Admin", "Accountant", "Viewer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            string AdminEmail = "Admin@qtec.com";
            string AdminPassword = "Admin@1234";

            if (await userManager.FindByEmailAsync(AdminEmail) == null)
            {
                var adminUser = new IdentityUser { UserName = AdminEmail, Email = AdminEmail };
                var result = await userManager.CreateAsync(adminUser, AdminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

        }

        public static async Task SeedModules(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            var modules = new[] { "ChartOfAccounts", "Vouchers", "Reports" };
            foreach (var module in modules) {
                var command = new SqlCommand("IF NOT EXISTS (SELECT * FROM Modules WHERE Name = @Name) INSERT INTO Modules (Name) VALUES (@Name)", connection);
                command.Parameters.AddWithValue("@Name", module);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

}
