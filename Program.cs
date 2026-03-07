using BlazorSocial.Client.Models;
using BlazorSocial.Components;
using BlazorSocial.Components.Account;
using BlazorSocial.Data;
using BlazorSocial.Data.Entities;
using BlazorSocial.Extensions;
using BlazorSocial.Services;
using BlazorSocial.Services.DataGeneratorService;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddFluentUIComponents();

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

var contentDbPath = Path.Combine(builder.Environment.ContentRootPath, "ContentDatabase.mdf");
var connectionString =
    $"Server=(localdb)\\mssqllocaldb;Database=ContentDatabase;AttachDbFilename={contentDbPath};Trusted_Connection=True;MultipleActiveResultSets=true";
builder.Services.AddDbContext<ContentDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContextFactory<ContentDbContext>(options =>
        options.UseSqlServer(connectionString),
    ServiceLifetime.Scoped);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<SocialUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole<UserId>>()
    .AddEntityFrameworkStores<ContentDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<SocialUser>, IdentityNoOpEmailSender>();

builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<DataGeneratorService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var contentDb = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
    contentDb.Database.EnsureCreated();
}

await app.SeedRolesAsync();
await app.SeedAdminUserAsync();
await app.SeedDevelopmentDataAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//var myService = app.Services.GetRequiredService<MetaService>();

app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ViewPostDto).Assembly);

app.MapGet("/api/posts",
    async (int startIndex, int count, IDbContextFactory<ContentDbContext> dbContextFactory, CancellationToken ct) =>
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

            //await Task.Delay(2000, ct);

            var dbPosts = await dbContext.Posts
                .Include(post => post.Author)
                .OrderByDescending(post => post.PostDate)
                .Skip(startIndex)
                .Take(count)
                .ToListAsync(ct);


            var posts = dbPosts.Select(post => new
            {
                PostId = post.Id.Value,
                post.Title,
                post.Content,
                post.PostDate,
                PostType = post.PostType.Name,
                AuthorName = post.Author?.UserName ?? "Unknown"
            });

            return Results.Ok(posts);
        }
        catch (OperationCanceledException)
        {
            return Results.StatusCode(499);
        }
        catch (InvalidOperationException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
    });

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();