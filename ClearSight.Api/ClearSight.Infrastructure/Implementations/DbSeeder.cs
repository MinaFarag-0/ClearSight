using ClearSight.Core.Models;
using ClearSight.Core.Mosels;
using ClearSight.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ClearSight.Infrastructure.Implementations
{
    public static class DbSeeder
    {
        public static async Task SeedAdminsAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Ensure Admin role exists
            var roleName = "Admin";
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // List of admin seed data
            var adminUsers = new List<(string Email, string Password)>
            {
                ("admin1@example.com", "Admin@123" ),
                ("admin2@example.com", "Admin@123" )
            };

            foreach (var (email, password) in adminUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(email);
                if (existingUser == null)
                {
                    var user = new User
                    {
                        UserName = email,
                        FullName = email,
                        Email = email,
                        EmailConfirmed = true,
                        ProfileImagePath = "http://res.cloudinary.com/dxpkckl5t/image/upload/v1737133446/temp/crhf7yb1o7bl71har1bb.png"
                    };

                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, roleName);

                        // Add to Admin table with same Id
                        var admin = new Admin
                        {
                            AdminId = user.Id,
                        };
                        context.Admins.Add(admin);
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }

}
