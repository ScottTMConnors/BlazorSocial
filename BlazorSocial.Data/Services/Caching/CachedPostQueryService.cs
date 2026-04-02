using System.Text.Json;
using BlazorSocial.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace BlazorSocial.Data.Services.Caching;

/// <summary>
/// Caching decorator for IPostQueryService. Caches the base feed data (no per-user state)
/// and overlays per-user vote state separately — so the cache works for ALL users.
/// </summary>
public class CachedPostQueryService(
    PostQueryService inner,
    IDbContextFactory<ContentDbContext> dbContextFactory,
    IDistributedCache cache) : IPostQueryService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<List<ViewPostDto>> GetPostsAsync(UserId? currentUserId, int startIndex, int count, CancellationToken ct)
    {
        // Step 1: get base feed from cache (always fetched without per-user data)
        var key = CacheKeys.PostFeed(startIndex, count);
        List<ViewPostDto> posts;

        var cached = await cache.GetStringAsync(key, ct);
        if (cached is not null)
        {
            posts = JsonSerializer.Deserialize<List<ViewPostDto>>(cached, JsonOptions) ?? [];
        }
        else
        {
            posts = await inner.GetPostsAsync(null, startIndex, count, ct);
            await cache.SetStringAsync(key, JsonSerializer.Serialize(posts, JsonOptions),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) }, ct);
        }

        // Step 2: overlay per-user votes if authenticated
        if (currentUserId is not null && posts.Count > 0)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(ct);
            var postIds = posts.Select(p => p.PostId).ToList();
            var userVotes = await db.Votes
                .Where(v => postIds.Contains(v.PostId) && v.UserId == currentUserId && v.IsActive)
                .Select(v => new { v.PostId, Vote = v.IsUpvote ? 1 : -1 })
                .ToDictionaryAsync(v => v.PostId, v => v.Vote, ct);

            foreach (var post in posts)
            {
                if (userVotes.TryGetValue(post.PostId, out var vote))
                    post.CurrentUserVote = vote;
            }
        }

        return posts;
    }

    public async Task<PostDetailsDto?> GetPostByIdAsync(PostId id, UserId? currentUserId, CancellationToken ct)
    {
        // Step 1: get base detail from cache (without per-user vote)
        var key = CacheKeys.PostDetail(id);
        PostDetailsDto? row;

        var cached = await cache.GetStringAsync(key, ct);
        if (cached is not null)
        {
            row = JsonSerializer.Deserialize<PostDetailsDto>(cached, JsonOptions);
        }
        else
        {
            row = await inner.GetPostByIdAsync(id, null, ct);
            if (row is not null)
            {
                await cache.SetStringAsync(key, JsonSerializer.Serialize(row, JsonOptions),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) }, ct);
            }
        }

        if (row is null) return null;

        // Step 2: overlay per-user vote if authenticated
        if (currentUserId is not null)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(ct);
            var vote = await db.Votes
                .Where(v => v.PostId == id && v.UserId == currentUserId && v.IsActive)
                .Select(v => v.IsUpvote ? 1 : -1)
                .FirstOrDefaultAsync(ct);

            row = row with { CurrentUserVote = vote };
        }

        return row;
    }
}
