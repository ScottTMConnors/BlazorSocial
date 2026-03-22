using Microsoft.AspNetCore.Identity;

namespace BlazorSocial.Auth.Extensions;

public static class SeedingExtensions
{
    public static async Task SeedAuthAdminAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<UserId>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthUser>>();

        // Seed roles
        string[] roles = ["Admin"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<UserId> { Id = UserId.New(), Name = role });
            }
        }

        // Seed admin user
        const string adminEmail = "admin@blazorsocial.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AuthUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Admin",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
