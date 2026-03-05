using BlazorSocial.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorSocial.Data;

public static class ContentDbContextExtensions
{
    /// <summary>
    ///     Finds an entity by its strongly-typed <see cref="UniqueId{TId}" /> primary key.
    ///     Usage: <c>await dbContext.Posts.Get(postId)</c>
    /// </summary>
    public static Task<TEntity?> GetAsync<TEntity, TId>(this IQueryable<TEntity> dbSet, TId id)
        where TEntity : class, IEntity<TId>
        where TId : UniqueId<TId>, new()
    {
        return dbSet.FirstOrDefaultAsync(e => e.Id == id);
    }

    extension(ContentDbContext dbContext)
    {
        public async Task RecordUserViewAsync(PostId postId, string? ipAddress, UserId? userId)
        {
            var metadata = await dbContext.PostMetadatas.FirstOrDefaultAsync(m => m.PostId == postId);
            if (metadata != null)
            {
                metadata.ViewCount += 1;
                dbContext.PostMetadatas.Update(metadata);
            }
            else
            {
                metadata = new PostMetadata(postId)
                {
                    ViewCount = 1
                };
                dbContext.PostMetadatas.Add(metadata);
            }

            dbContext.Views.Add(new View
            {
                PostId = postId,
                UserId = userId,
                ViewDateTime = DateTime.Now
            });

            await dbContext.SaveChangesAsync();
        }

        public async Task UpdatePostMetadataAsync()
        {
            var voteStats = await dbContext.Votes
                .Where(v => v.IsActive)
                .GroupBy(v => v.PostId)
                .Select(g => new
                {
                    PostId = g.Key,
                    Upvotes = g.Count(v => v.IsUpvote),
                    Downvotes = g.Count(v => !v.IsUpvote),
                    TotalVotes = g.Count()
                })
                .ToDictionaryAsync(v => v.PostId);

            var viewCounts = await dbContext.Views
                .GroupBy(v => v.PostId)
                .Select(g => new
                {
                    PostId = g.Key,
                    ViewCount = g.Count()
                })
                .ToDictionaryAsync(v => v.PostId);

            var allMetadata = await dbContext.PostMetadatas.ToListAsync();

            foreach (var meta in allMetadata)
            {
                var hasVotes = voteStats.TryGetValue(meta.PostId, out var votes);
                var hasViews = viewCounts.TryGetValue(meta.PostId, out var views);

                meta.Upvotes = hasVotes ? votes.Upvotes : 0;
                meta.Downvotes = hasVotes ? votes.Downvotes : 0;
                meta.TotalVotes = hasVotes ? votes.TotalVotes : 0;
                meta.NetVotes = meta.Upvotes - meta.Downvotes;
                meta.ViewCount = hasViews ? views.ViewCount : 0;
            }

            await dbContext.SaveChangesAsync();
        }
    }
}