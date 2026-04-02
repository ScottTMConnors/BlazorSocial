using BlazorSocial.Data.BackgroundJobs;
using BlazorSocial.Data.Entities;
using BlazorSocial.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorSocial.Data.Services;

public class CommentService(
    IDbContextFactory<ContentDbContext> dbContextFactory,
    MetadataEventQueue queue) : ICommentService
{
    public async Task<List<CommentDto>> GetCommentsAsync(PostId postId, int startIndex, int count, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        return await dbContext.Comments
            .AsNoTracking()
            .Where(c => c.PostId == postId)
            .OrderByDescending(c => c.PostDate)
            .Skip(startIndex)
            .Take(count)
            .Select(c => new CommentDto
            {
                AuthorName = c.Author != null ? c.Author.DisplayName : "Unknown",
                PostDate = c.PostDate,
                Content = c.Content
            })
            .ToListAsync(ct);
    }

    public async Task AddCommentAsync(PostId postId, UserId authorId, string content, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        var comment = new Comment
        {
            PostId = postId,
            AuthorID = authorId,
            Content = content,
            PostDate = DateTime.Now
        };
        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync(ct);

        queue.Enqueue(new CommentAdded(postId));
    }
}
