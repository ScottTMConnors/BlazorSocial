using BlazorSocial.Data.BackgroundJobs;
using BlazorSocial.Data.Services;
using BlazorSocial.Data.Services.Caching;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorSocial.Data;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBlazorSocialDataServices()
        {
            // Singleton event queues — each dedicated to a single consumer, eliminating race conditions
            services.AddSingleton<MetadataEventQueue>();
            services.AddSingleton<ViewEventQueue>();

            // Background workers that drain their respective queues
            services.AddHostedService<MetadataUpdateWorker>();
            services.AddHostedService<ViewTrackingWorker>();

            // Register concrete implementations so cached decorators can inject them directly
            services.AddScoped<PostQueryService>();
            services.AddScoped<PostCommandService>();
            services.AddScoped<CommentService>();

            // Cached decorator for query service — works for ALL users (base feed cached, votes overlaid)
            services.AddScoped<IPostQueryService, CachedPostQueryService>();

            // Command service — no caching on the write side
            services.AddScoped<IPostCommandService, PostCommandService>();

            // Comment service with caching decorator
            services.AddScoped<ICommentService, CachedCommentService>();

            // Vote and view tracking — writes only, no caching
            services.AddScoped<IVoteService, VoteService>();
            services.AddScoped<IViewTrackingService, ViewTrackingService>();

            // Data generator service for development seeding
            services.AddScoped<DataGeneratorService>();

            return services;
        }
    }
}