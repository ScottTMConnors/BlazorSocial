using BlazorSocial.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorSocial.Data.Services;

/// <summary>
///     Read-model-backed query service. Reads from the PostReadModels view (joins Posts + PostMetadatas + Users)
///     and computes per-user vote state in a single LEFT JOIN query when a userId is provided.
/// </summary>
public class PostQueryService(IDbContextFactory<ContentDbContext> dbContextFactory) : IPostQueryService
{
    public async Task<List<ViewPostDto>> GetPostsAsync(UserId? currentUserId, int startIndex, int count,
        CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        if (currentUserId is null)
        {
            return await db.PostReadModels
                .AsNoTracking()
                .OrderByDescending(r => r.PostDate)
                .Skip(startIndex)
                .Take(count)
                .Select(r => new ViewPostDto
                {
                    PostId = r.PostId,
                    Title = r.Title,
                    Content = r.Content,
                    PostDate = r.PostDate,
                    PostType = r.PostType,
                    AuthorName = r.AuthorName,
                    Score = r.NetVotes,
                    CommentCount = r.CommentCount,
                    CurrentUserVote = 0
                })
                .ToListAsync(ct);
        }

        return await (
            from r in db.PostReadModels.AsNoTracking()
                .OrderByDescending(r => r.PostDate)
                .Skip(startIndex)
                .Take(count)
            join v in db.Votes.Where(v => v.UserId == currentUserId && v.IsActive)
                on r.PostId equals v.PostId into voteGroup
            from vote in voteGroup.DefaultIfEmpty()
            select new ViewPostDto
            {
                PostId = r.PostId,
                Title = r.Title,
                Content = r.Content,
                PostDate = r.PostDate,
                PostType = r.PostType,
                AuthorName = r.AuthorName,
                Score = r.NetVotes,
                CommentCount = r.CommentCount,
                CurrentUserVote = vote == null ? 0 : vote.IsUpvote ? 1 : -1
            }
        ).ToListAsync(ct);
    }

    public async Task<PostDetailsDto?> GetPostByIdAsync(PostId id, UserId? currentUserId, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        if (currentUserId is null)
        {
            return await db.PostReadModels
                .AsNoTracking()
                .Where(r => r.PostId == id)
                .Select(r => new PostDetailsDto
                {
                    PostId = r.PostId,
                    Title = r.Title,
                    Content = r.Content,
                    PostDate = r.PostDate,
                    ViewCount = r.ViewCount,
                    NetVotes = r.NetVotes,
                    Upvotes = r.Upvotes,
                    Downvotes = r.Downvotes,
                    IsAuthenticated = false,
                    CurrentUserVote = 0,
                    CommentCount = r.CommentCount
                })
                .FirstOrDefaultAsync(ct);
        }

        return await (
            from r in db.PostReadModels.AsNoTracking().Where(r => r.PostId == id)
            join v in db.Votes.Where(v => v.UserId == currentUserId && v.IsActive)
                on r.PostId equals v.PostId into voteGroup
            from vote in voteGroup.DefaultIfEmpty()
            select new PostDetailsDto
            {
                PostId = r.PostId,
                Title = r.Title,
                Content = r.Content,
                PostDate = r.PostDate,
                ViewCount = r.ViewCount,
                NetVotes = r.NetVotes,
                Upvotes = r.Upvotes,
                Downvotes = r.Downvotes,
                IsAuthenticated = true,
                CurrentUserVote = vote == null ? 0 : vote.IsUpvote ? 1 : -1,
                CommentCount = r.CommentCount
            }
        ).FirstOrDefaultAsync(ct);
    }
}