using BlazorSocial.Data.Entities;
using BlazorSocial.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorSocial.Data.Services;

public class PostService(IDbContextFactory<ContentDbContext> dbContextFactory) : IPostService
{
    public async Task<List<PostRow>> GetPostsAsync(UserId? currentUserId, int startIndex, int count, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        return await dbContext.Posts
            .Include(p => p.Author)
            .Include(p => p.PostMetadata)
            .Include(p => p.Votes)
            .OrderByDescending(post => post.PostDate)
            .Skip(startIndex)
            .Take(count)
            .Select(post => new PostRow
            {
                PostId = post.Id,
                Title = post.Title,
                Content = post.Content,
                PostDate = post.PostDate,
                PostType = post.PostType.ToString(),
                AuthorName = post.Author != null ? post.Author.UserName ?? "Unknown" : "Unknown",
                CommentCount = post.PostMetadata.CommentCount,
                Score = post.PostMetadata.NetVotes,
                CurrentUserVote = currentUserId == null
                    ? 0
                    : post.Votes
                        .Where(v => v.UserId == currentUserId)
                        .Select(v => v.IsActive ? v.IsUpvote ? 1 : -1 : 0)
                        .FirstOrDefault()
            })
            .ToListAsync(ct);
    }

    public async Task<PostDetailRow?> GetPostByIdAsync(PostId id, UserId? currentUserId, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        return await dbContext.Posts
            .Where(p => p.Id == id)
            .Select(post => new PostDetailRow
            {
                PostId = post.Id,
                Title = post.Title,
                Content = post.Content,
                PostDate = post.PostDate,
                ViewCount = post.PostMetadata.ViewCount,
                NetVotes = post.PostMetadata.NetVotes,
                Upvotes = post.PostMetadata.Upvotes,
                Downvotes = post.PostMetadata.Downvotes,
                CommentCount = post.PostMetadata.CommentCount,
                CurrentUserVote = currentUserId == null
                    ? 0
                    : post.Votes
                        .Where(v => v.UserId == currentUserId)
                        .Select(v => v.IsActive ? v.IsUpvote ? 1 : -1 : 0)
                        .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PostId> CreatePostAsync(string title, string content, UserId authorId, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var post = new Post(title, content, authorId, DateTime.UtcNow, PostType.Text);
        db.Posts.Add(post);
        await db.SaveChangesAsync(ct);
        return post.Id;
    }
}
