using System.Text.Json;
using BlazorSocial.Data.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace BlazorSocial.Data.Services.Caching;

public class CachedPostService(PostService inner, IDistributedCache cache) : IPostService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<List<PostRow>> GetPostsAsync(UserId? currentUserId, int startIndex, int count, CancellationToken ct)
    {
        // Only cache anonymous feed — per-user vote state means we skip caching for authenticated users
        if (currentUserId is not null)
        {
            return await inner.GetPostsAsync(currentUserId, startIndex, count, ct);
        }

        var key = CacheKeys.PostFeed(startIndex, count);
        var cached = await cache.GetStringAsync(key, ct);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<List<PostRow>>(cached, JsonOptions) ?? [];
        }

        var result = await inner.GetPostsAsync(null, startIndex, count, ct);

        await cache.SetStringAsync(key, JsonSerializer.Serialize(result, JsonOptions),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) }, ct);

        return result;
    }

    public async Task<PostDetailRow?> GetPostByIdAsync(PostId id, UserId? currentUserId, CancellationToken ct)
    {
        // Only cache anonymous lookups — per-user vote state differs per user
        if (currentUserId is not null)
        {
            return await inner.GetPostByIdAsync(id, currentUserId, ct);
        }

        var key = CacheKeys.PostDetail(id);
        var cached = await cache.GetStringAsync(key, ct);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<PostDetailRow>(cached, JsonOptions);
        }

        var result = await inner.GetPostByIdAsync(id, null, ct);
        if (result is not null)
        {
            await cache.SetStringAsync(key, JsonSerializer.Serialize(result, JsonOptions),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) }, ct);
        }

        return result;
    }

    public async Task<PostId> CreatePostAsync(string title, string content, UserId authorId, CancellationToken ct)
    {
        var postId = await inner.CreatePostAsync(title, content, authorId, ct);
        // Invalidate feed cache so new post appears promptly (best effort — short TTL also handles it)
        // We can't enumerate all feed keys, so we rely on the short 30s TTL for feed expiry
        return postId;
    }
}
