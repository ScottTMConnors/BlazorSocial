using BlazorSocial.Data.BackgroundJobs;
using BlazorSocial.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorSocial.Data.Services;

public class VoteService(
    IDbContextFactory<ContentDbContext> dbContextFactory,
    PostEventQueue queue) : IVoteService
{
    public async Task VoteAsync(PostId postId, UserId userId, bool isUpvote, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        var vote = await dbContext.Votes
            .FirstOrDefaultAsync(v => v.PostId == postId && v.UserId == userId, ct);

        if (vote is null)
        {
            vote = new Vote
            {
                PostId = postId,
                UserId = userId,
                IsUpvote = isUpvote,
                IsActive = true,
                VoteDate = DateTime.Now
            };
            dbContext.Votes.Add(vote);
        }
        else
        {
            if (vote.IsUpvote == isUpvote && vote.IsActive)
            {
                vote.IsActive = false;
            }
            else
            {
                vote.IsActive = true;
                vote.IsUpvote = isUpvote;
            }

            vote.VoteDate = DateTime.Now;
            dbContext.Votes.Update(vote);
        }

        await dbContext.SaveChangesAsync(ct);

        // Enqueue metadata update instead of doing it inline
        queue.Enqueue(new VoteRecorded(postId));
    }
}
