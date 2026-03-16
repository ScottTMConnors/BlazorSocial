#if DEBUG
using BlazorSocial.Services.DataGeneratorService;
#endif
using BlazorSocial.Components;
using BlazorSocial.Components.Account;
using BlazorSocial.Data;
using BlazorSocial.Data.Entities;
using BlazorSocial.Extensions;
using BlazorSocial.ServiceDefaults;
using BlazorSocial.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using BlazorSocialClient = BlazorSocial.Client._Imports;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddFluentUIComponents(options =>
{
    options.MarkupSanitized.SanitizeInlineStyle = value =>
    {
        if (value.Contains("url(", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("expression(", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("javascript:", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("@import", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("-moz-binding", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"The provided CSS inline style contains potentially unsafe content: {value}");
        }

        return value;
    };
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddAuthorization();

if (builder.Environment.IsEnvironment("Testing"))
{
    // Use SQLite in-memory for test runs. The connection is kept alive by
    // TestWebAppFactory so the named database persists for the whole test run.
    const string testConnectionString = "DataSource=BlazorSocialTest;Mode=Memory;Cache=Shared";
    builder.Services.AddDbContext<ContentDbContext>(options => options.UseSqlite(testConnectionString));
    builder.Services.AddDbContextFactory<ContentDbContext>(options => options.UseSqlite(testConnectionString));
}
else
{
    builder.AddSqlServerDbContext<ContentDbContext>("ContentDatabase");
    builder.Services.AddPooledDbContextFactory<ContentDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("ContentDatabase")));
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<SocialUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole<UserId>>()
    .AddEntityFrameworkStores<ContentDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<SocialUser>, IdentityNoOpEmailSender>();

builder.Services.AddOpenApi();

builder.Services.AddScoped<CurrentUserService>();
#if DEBUG
builder.Services.AddScoped<DataGeneratorService>();
#endif

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var contentDb = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
    contentDb.Database.EnsureCreated();
}

await app.SeedRolesAsync();
await app.SeedAdminUserAsync();
#if DEBUG
await app.SeedDevelopmentDataAsync();
#endif

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "BlazorSocial API"));
}
else
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorSocialClient).Assembly);

app.MapPostApiEndpoints();
app.MapAccountApiEndpoints();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapDefaultEndpoints();

app.Run();