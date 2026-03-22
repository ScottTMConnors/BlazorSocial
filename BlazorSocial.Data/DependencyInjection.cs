using BlazorSocial.Data.BackgroundJobs;
using BlazorSocial.Data.Services;
using BlazorSocial.Data.Services.Caching;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorSocial.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddBlazorSocialDataServices(this IServiceCollection services)
    {
        // Singleton event queue — shared across all scopes in this process
        services.AddSingleton<PostEventQueue>();

        // Background workers that drain the queue
        services.AddHostedService<MetadataUpdateWorker>();
        services.AddHostedService<ViewTrackingWorker>();

        // Register concrete implementations so cached decorators can inject them directly
        services.AddScoped<PostService>();
        services.AddScoped<CommentService>();

        // Cached decorators fulfil the public interface contracts
        services.AddScoped<IPostService, CachedPostService>();
        services.AddScoped<ICommentService, CachedCommentService>();

        // Vote and view tracking — writes only, no caching
        services.AddScoped<IVoteService, VoteService>();
        services.AddScoped<IViewTrackingService, ViewTrackingService>();

        return services;
    }
}
