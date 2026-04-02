using BlazorSocial.Api.Extensions;
using BlazorSocial.Data;
using BlazorSocial.Data.Entities;
using BlazorSocial.Data.Services;
using BlazorSocial.ServiceDefaults;
using BlazorSocial.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var isTestingEnv = builder.Environment.IsEnvironment("Testing");

// JWT signing key — use placeholder for tests (no real JWT validation needed in test mode
// since the InternalProxyAuthHandler pattern is replaced by X-User-Id header injection)
var jwtKey = builder.Configuration["Jwt:SigningKey"] ?? "test-signing-key-placeholder-for-integration-tests";
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

if (isTestingEnv)
{
    const string testConnectionString = "DataSource=BlazorSocialTest;Mode=Memory;Cache=Shared";
    builder.Services.AddDbContext<ContentDbContext>(options => options.UseSqlite(testConnectionString));
    builder.Services.AddDbContextFactory<ContentDbContext>(options => options.UseSqlite(testConnectionString));
    builder.Services.AddBlazorSocialDataServices();
    builder.Services.AddDistributedMemoryCache();
}
else
{
    builder.AddSqlServerDbContext<ContentDbContext>("ContentDatabase");
    builder.Services.AddPooledDbContextFactory<ContentDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("ContentDatabase")));
    builder.Services.AddBlazorSocialDataServices();
    builder.AddRedisDistributedCache("cache");
}

builder.Services.AddOpenApi();

var app = builder.Build();

if (isTestingEnv)
{
    // Ensure schema is created and seed a test admin SocialUser for integration tests
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
    db.Database.EnsureCreated();

    const string adminEmail = "admin@blazorsocial.com";
    if (!await db.Users.AnyAsync(u => u.Email == adminEmail))
    {
        db.Users.Add(new SocialUser(UserId.New())
        {
            DisplayName = "Admin",
            UserName = adminEmail,
            Email = adminEmail
        });
        await db.SaveChangesAsync();
    }

    db.Database.ExecuteSqlRaw("""
        CREATE VIEW IF NOT EXISTS PostReadModels AS
        SELECT
            p.Id        AS PostId,
            p.Title,
            p.Content,
            p.PostDate,
            CASE p.PostType
                WHEN 1 THEN 'Text'
                WHEN 2 THEN 'Image'
                WHEN 3 THEN 'Video'
                WHEN 4 THEN 'Link'
                WHEN 5 THEN 'EmbeddedVideo'
                ELSE 'Unknown'
            END         AS PostType,
            COALESCE(u.UserName, 'Unknown') AS AuthorName,
            COALESCE(pm.Upvotes,      0)    AS Upvotes,
            COALESCE(pm.Downvotes,    0)    AS Downvotes,
            COALESCE(pm.NetVotes,     0)    AS NetVotes,
            COALESCE(pm.ViewCount,    0)    AS ViewCount,
            COALESCE(pm.CommentCount, 0)    AS CommentCount
        FROM Posts p
        LEFT JOIN PostMetadatas pm ON pm.PostId = p.Id
        LEFT JOIN Users          u  ON u.Id     = p.AuthorId
        """);
}

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
    db.Database.EnsureCreated();

    db.Database.ExecuteSqlRaw("""
        CREATE OR ALTER VIEW PostReadModels AS
        SELECT
            p.Id        AS PostId,
            p.Title,
            p.Content,
            p.PostDate,
            CASE p.PostType
                WHEN 1 THEN 'Text'
                WHEN 2 THEN 'Image'
                WHEN 3 THEN 'Video'
                WHEN 4 THEN 'Link'
                WHEN 5 THEN 'EmbeddedVideo'
                ELSE 'Unknown'
            END         AS PostType,
            COALESCE(u.UserName, 'Unknown') AS AuthorName,
            COALESCE(pm.Upvotes,      0)    AS Upvotes,
            COALESCE(pm.Downvotes,    0)    AS Downvotes,
            COALESCE(pm.NetVotes,     0)    AS NetVotes,
            COALESCE(pm.ViewCount,    0)    AS ViewCount,
            COALESCE(pm.CommentCount, 0)    AS CommentCount
        FROM Posts p
        LEFT JOIN PostMetadatas pm ON pm.PostId = p.Id
        LEFT JOIN Users          u  ON u.Id     = p.AuthorId
        """);

    // Generate sample Reddit data if database is empty (UseReddit=true by default in DataGeneratorService)
    var postCount = await db.Posts.CountAsync();
    if (postCount == 0)
    {
        Console.WriteLine("Database is empty. Generating sample data from Reddit...");
        var dataGenerator = scope.ServiceProvider.GetRequiredService<DataGeneratorService>();
        // numberOfPosts: ignored when using Reddit (pulls ~100 from /r/confession)
        // numberOfUsers: users created to associate with posts
        // numberOfInteractions: base number for views/votes/comments per post
        await dataGenerator.GenerateData(
            numberOfPosts: 100,
            numberOfUsers: 50,
            numberOfInteractions: 10
        );
        Console.WriteLine("Sample data generation complete.");
    }

    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "BlazorSocial API"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Lazy SocialUser creation: on first authenticated request, ensure a SocialUser exists
// in ContentDbContext for this JWT identity. This removes the Auth→Api coupling on register.
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var userId = context.GetCurrentUserId();
        if (userId is not null)
        {
            var db = context.RequestServices.GetRequiredService<IDbContextFactory<ContentDbContext>>();
            await using var dbContext = await db.CreateDbContextAsync();
            if (!await dbContext.Users.AnyAsync(u => u.Id == userId))
            {
                var displayName = context.User.FindFirstValue("display_name") ?? "";
                var userName = context.User.FindFirstValue(ClaimTypes.Email) ?? "";
                dbContext.Users.Add(new SocialUser(userId) { DisplayName = displayName, UserName = userName });
                await dbContext.SaveChangesAsync();
            }
        }
    }
    await next(context);
});

app.MapPostApiEndpoints();

app.MapDefaultEndpoints();

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
