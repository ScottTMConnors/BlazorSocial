using BlazorSocial.Data;
using BlazorSocial.Data.Entities;
using BlazorSocial.Services.DataGeneratorService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
#if DEBUG
        public async Task<WebApplication> SeedDevelopmentDataAsync()
        {
            if (!app.Environment.IsDevelopment())
            {
                return app;
            }

            using var scope = app.Services.CreateScope();
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ContentDbContext>>();

            await using var dbContext = await dbContextFactory.CreateDbContextAsync();
            if (await dbContext.Posts.AnyAsync())
            {
                return app;
            }

            var generator = scope.ServiceProvider.GetRequiredService<DataGeneratorService>();
            await generator.GenerateData(numberOfUsers: 100, numberOfPosts: 100, numberOfInteractions: 999);

            return app;
        }
#endif
    }
}