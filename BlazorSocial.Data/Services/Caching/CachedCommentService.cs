using System.Text.Json;
using BlazorSocial.Shared.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace BlazorSocial.Data.Services.Caching;

public class CachedCommentService(CommentService inner, IDistributedCache cache) : ICommentService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<List<CommentDto>> GetCommentsAsync(PostId postId, int startIndex, int count, CancellationToken ct)
    {
        var key = CacheKeys.PostComments(postId, startIndex, count);
        var cached = await cache.GetStringAsync(key, ct);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<List<CommentDto>>(cached, JsonOptions) ?? [];
        }

        var result = await inner.GetCommentsAsync(postId, startIndex, count, ct);

        await cache.SetStringAsync(key, JsonSerializer.Serialize(result, JsonOptions),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) }, ct);

        return result;
    }

    public async Task AddCommentAsync(PostId postId, UserId authorId, string content, CancellationToken ct)
    {
        await inner.AddCommentAsync(postId, authorId, content, ct);
        // Invalidate comment cache for page 0 — the most common view after posting
        var key = CacheKeys.PostComments(postId, 0, 10);
        await cache.RemoveAsync(key, ct);
    }
}
