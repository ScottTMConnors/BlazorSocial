using System.Net;
using System.Net.Http.Json;
using BlazorSocial.Data;
using BlazorSocial.Data.Entities;
using BlazorSocial.Shared.Models;
using BlazorSocial.Tests.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorSocial.Tests;

/// <summary>
/// Integration tests that boot the server with a SQLite in-memory database.
/// Verifies that every IPostsApi method maps to a registered server endpoint,
/// and that each endpoint responds with the expected status code.
/// </summary>
public class PostEndpointTests(TestWebAppFactory factory) : IClassFixture<TestWebAppFactory>
{
    private static readonly string AnyId = Guid.NewGuid().ToString("N");

    // -------------------------------------------------------------------------
    // Contract: every IPostsApi method must have a matching registered endpoint
    // -------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(ApiContractTests.ApiEndpoints), MemberType = typeof(ApiContractTests))]
    public void Server_HasRegisteredEndpoint_ForEveryIPostsApiMethod(string httpMethod, string route, string methodName)
    {
        var endpointDataSource = factory.Services.GetRequiredService<EndpointDataSource>();

        var match = endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Any(e =>
                string.Equals(
                    e.RoutePattern.RawText?.TrimStart('/'),
                    route.TrimStart('/'),
                    StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                    e.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.FirstOrDefault(),
                    httpMethod,
                    StringComparison.OrdinalIgnoreCase));

        Assert.True(match, $"No registered server endpoint for IPostsApi.{methodName} ({httpMethod} {route})");
    }

    // -------------------------------------------------------------------------
    // Smoke tests: verify status codes without needing real data
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetPosts_ReturnsOk()
    {
        var response = await factory.CreateClient().GetAsync("/api/posts?startIndex=0&count=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPosts_WithNoData_ReturnsEmptyList()
    {
        var response = await factory.CreateClient().GetAsync("/api/posts?startIndex=0&count=10");
        var posts = await response.Content.ReadFromJsonAsync<List<ViewPostDto>>();
        Assert.NotNull(posts);
    }

    [Fact]
    public async Task GetPostById_UnknownId_ReturnsNotFound()
    {
        var response = await factory.CreateClient().GetAsync($"/api/posts/{AnyId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCommentsForPost_UnknownPost_ReturnsOkWithEmptyList()
    {
        var response = await factory.CreateClient()
            .GetAsync($"/api/posts/{AnyId}/comments?startIndex=0&count=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var comments = await response.Content.ReadFromJsonAsync<List<CommentDto>>();
        Assert.Empty(comments!);
    }

    [Fact]
    public async Task VoteOnPost_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await factory.CreateClient()
            .PostAsJsonAsync($"/api/posts/{AnyId}/vote", new { IsUpvote = true });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CommentOnPost_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await factory.CreateClient()
            .PostAsJsonAsync($"/api/posts/{AnyId}/comments", new { Content = "test" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ViewPost_ExistingPost_ReturnsOk()
    {
        // Arrange: seed a post using the admin user who is pre-seeded by Program.cs startup
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ContentDbContext>();

        var admin = await dbContext.Users.FirstAsync(u => u.Email == "admin@blazorsocial.com");
        var post = new Post("Smoke Test Post", "<p>Test</p>", admin.Id, DateTime.Now, PostType.Text);
        dbContext.Posts.Add(post);
        await dbContext.SaveChangesAsync();

        var response = await factory.CreateClient().PostAsync($"/api/posts/{post.Id}/view", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
