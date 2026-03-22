using System.Reflection;
using BlazorSocial.Shared.Models;
using Refit;

namespace BlazorSocial.Tests;

/// <summary>
/// Pure reflection tests — no server required.
/// Verifies the IPostsApi interface and ApiRoute.Templates stay in sync.
/// These fail immediately when a route is added to one side but not the other.
/// </summary>
public class ApiContractTests
{
    private static readonly IReadOnlySet<string> AllTemplates =
        typeof(ApiRoute.Templates)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral)
            .Select(f => (string)f.GetValue(null)!)
            .ToHashSet();

    /// <summary>
    /// Provides one row per IPostsApi method: (httpMethod, routePath, methodName).
    /// Shared with PostEndpointTests so the two test classes stay in sync.
    /// </summary>
    public static TheoryData<string, string, string> ApiEndpoints()
    {
        var data = new TheoryData<string, string, string>();
        foreach (var method in typeof(IPostsApi).GetMethods())
        {
            var get = method.GetCustomAttribute<GetAttribute>();
            var post = method.GetCustomAttribute<PostAttribute>();
            if (get is not null) data.Add("GET", get.Path, method.Name);
            else if (post is not null) data.Add("POST", post.Path, method.Name);
        }
        return data;
    }

    [Fact]
    public void IPostsApi_AllMethods_HaveRefitAttribute()
    {
        var missing = typeof(IPostsApi).GetMethods()
            .Where(m => m.GetCustomAttribute<GetAttribute>() is null
                     && m.GetCustomAttribute<PostAttribute>() is null)
            .Select(m => m.Name)
            .ToList();

        Assert.Empty(missing);
    }

    [Theory]
    [MemberData(nameof(ApiEndpoints))]
    public void IPostsApi_Route_ExistsInApiRouteTemplates(string httpMethod, string route, string methodName)
    {
        Assert.True(
            AllTemplates.Contains(route),
            $"IPostsApi.{methodName} ({httpMethod} {route}) has no matching constant in ApiRoute.Templates");
    }

    /// <summary>
    /// Routes consumed by non-Refit endpoints (e.g. Auth service)
    /// that should be excluded from the IPostsApi coverage check.
    /// </summary>
    private static readonly IReadOnlySet<string> NonRefitRoutes = new HashSet<string>
    {
        ApiRoute.Auth.Login,
        ApiRoute.Auth.Register
    };

    [Fact]
    public void ApiRouteTemplates_AllTemplates_UsedByAtLeastOneApiMethod()
    {
        var usedRoutes = typeof(IPostsApi).GetMethods()
            .Select(m => m.GetCustomAttribute<GetAttribute>()?.Path
                      ?? m.GetCustomAttribute<PostAttribute>()?.Path)
            .Where(p => p is not null)
            .ToHashSet()!;

        var unused = AllTemplates.Except(usedRoutes).Except(NonRefitRoutes).ToList();

        Assert.Empty(unused);
    }
}
