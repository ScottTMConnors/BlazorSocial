using BlazorSocial.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorSocial.Auth;

public class AuthDbContext(DbContextOptions<AuthDbContext> options)
    : IdentityDbContext<AuthUser, IdentityRole<UserId>, UserId>(options)
{
    protected override void ConfigureConventions(ModelConfigurationBuilder cfg)
    {
        cfg.Properties<UserId>().HaveConversion<UniqueIdConverter<UserId>>();
    }
}
