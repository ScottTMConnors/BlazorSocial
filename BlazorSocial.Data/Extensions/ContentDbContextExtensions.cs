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
            dbContext.Views.Add(new View
            {
                PostId = postId,
                UserId = userId,
                ViewDateTime = DateTime.Now
            });
            await dbContext.SaveChangesAsync();

            var updated = await dbContext.PostMetadatas
                .Where(m => m.PostId == postId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(m => m.ViewCount, m => m.ViewCount + 1));

            if (updated == 0)
            {
                dbContext.PostMetadatas.Add(new PostMetadata(postId) { ViewCount = 1 });
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateVoteMetadataAsync(PostId postId)
        {
            await dbContext.PostMetadatas
                .Where(m => m.PostId == postId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(m => m.Upvotes,
                        dbContext.Votes.Count(v => v.PostId == postId && v.IsActive && v.IsUpvote))
                    .SetProperty(m => m.Downvotes,
                        dbContext.Votes.Count(v => v.PostId == postId && v.IsActive && !v.IsUpvote))
                    .SetProperty(m => m.TotalVotes,
                        dbContext.Votes.Count(v => v.PostId == postId && v.IsActive))
                    .SetProperty(m => m.NetVotes,
                        dbContext.Votes.Where(v => v.PostId == postId && v.IsActive)
                            .Sum(v => v.IsUpvote ? 1 : -1)));
        }

        public async Task UpdatePostMetadataAsync()
        {
            var postIds = await dbContext.PostMetadatas
                .Select(m => m.PostId)
                .ToListAsync();

            foreach (var postId in postIds)
            {
                await dbContext.UpdateVoteMetadataAsync(postId);

                var viewCount = await dbContext.Views.CountAsync(v => v.PostId == postId);
                var commentCount = await dbContext.Comments.CountAsync(c => c.PostId == postId);
                await dbContext.PostMetadatas
                    .Where(m => m.PostId == postId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(m => m.ViewCount, viewCount)
                        .SetProperty(m => m.CommentCount, commentCount));
            }
        }
    }
}
