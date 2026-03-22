using BlazorSocial.Auth;
using BlazorSocial.Auth.Extensions;
using BlazorSocial.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

if (builder.Environment.IsEnvironment("Testing"))
{
    const string testConnectionString = "DataSource=BlazorSocialAuthTest;Mode=Memory;Cache=Shared";
    builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlite(testConnectionString));
}
else
{
    builder.AddSqlServerDbContext<AuthDbContext>("AuthDatabase");
}

builder.Services.AddIdentityCore<AuthUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole<UserId>>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:SigningKey"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = "blazorsocial-auth",
            ValidateAudience = true,
            ValidAudience = "blazorsocial-api"
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.EnsureCreated();
}

await app.SeedAuthAdminAsync();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapDefaultEndpoints();

app.Run();
