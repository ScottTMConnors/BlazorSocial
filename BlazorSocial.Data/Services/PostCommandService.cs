using BlazorSocial.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorSocial.Data.Services;

/// <summary>
/// Write-side post service. Inserts the Post entity and its PostReadModel row
/// in the same SaveChanges call so new posts appear in the feed immediately.
/// </summary>
public class PostCommandService(IDbContextFactory<ContentDbContext> dbContextFactory) : IPostCommandService
{
    public async Task<PostId> CreatePostAsync(string title, string content, UserId authorId, CancellationToken ct)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);

        var post = new Post(title, content, authorId, DateTime.UtcNow, PostType.Text);

        db.Posts.Add(post);

        await db.SaveChangesAsync(ct);
        return post.Id;
    }
}
