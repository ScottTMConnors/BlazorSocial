using BlazorSocial.Data;
using BlazorSocial.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace BlazorSocial.Extensions;

public static class IdentitySeedExtensions
{
    extension(WebApplication app)
    {
        public async Task<WebApplication> SeedRolesAsync()
        {
            using var scope = app.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<UserId>>>();

            string[] roles = ["Admin"];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<UserId> { Id = UserId.New(), Name = role });
                }
            }

            return app;
        }

        public async Task<WebApplication> SeedAdminUserAsync()
        {
            using var scope = app.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<SocialUser>>();

            var adminEmail = "admin@blazorsocial.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new SocialUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    DisplayName = "Admin"
                };
                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            return app;
        }
    }
}