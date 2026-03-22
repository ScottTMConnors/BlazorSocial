using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlazorSocial.Data.BackgroundJobs;

public sealed class MetadataUpdateWorker(
    PostEventQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<MetadataUpdateWorker> logger) : BackgroundService
{
    private const int FlushIntervalSeconds = 5;
    private const int FlushBatchSize = 100;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pendingVoteUpdates = new HashSet<PostId>();
        var pendingCommentUpdates = new HashSet<PostId>();
        var pendingNewPosts = new HashSet<PostId>();

        using var flushTimer = new PeriodicTimer(TimeSpan.FromSeconds(FlushIntervalSeconds));

        var readTask = ReadEventsAsync(queue, pendingVoteUpdates, pendingCommentUpdates, pendingNewPosts, stoppingToken);
        var flushTask = FlushPeriodicallyAsync(flushTimer, pendingVoteUpdates, pendingCommentUpdates, pendingNewPosts, stoppingToken);

        await Task.WhenAll(readTask, flushTask);
    }

    private static async Task ReadEventsAsync(
        PostEventQueue queue,
        HashSet<PostId> votes,
        HashSet<PostId> comments,
        HashSet<PostId> newPosts,
        CancellationToken ct)
    {
        await foreach (var evt in queue.ReadAllAsync(ct))
        {
            switch (evt)
            {
                case VoteRecorded(var postId):
                    votes.Add(postId);
                    break;
                case CommentAdded(var postId):
                    comments.Add(postId);
                    break;
                case PostCreated(var postId):
                    newPosts.Add(postId);
                    break;
            }
        }
    }

    private async Task FlushPeriodicallyAsync(
        PeriodicTimer timer,
        HashSet<PostId> votes,
        HashSet<PostId> comments,
        HashSet<PostId> newPosts,
        CancellationToken ct)
    {
        while (await timer.WaitForNextTickAsync(ct))
        {
            await FlushAsync(votes, comments, newPosts, ct);
        }
    }

    private async Task FlushAsync(
        HashSet<PostId> votes,
        HashSet<PostId> comments,
        HashSet<PostId> newPosts,
        CancellationToken ct)
    {
        if (votes.Count == 0 && comments.Count == 0 && newPosts.Count == 0)
        {
            return;
        }

        var voteSnapshot = votes.ToHashSet();
        var commentSnapshot = comments.ToHashSet();
        var newPostSnapshot = newPosts.ToHashSet();
        votes.Clear();
        comments.Clear();
        newPosts.Clear();

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ContentDbContext>>();
            await using var dbContext = await dbFactory.CreateDbContextAsync(ct);

            foreach (var postId in voteSnapshot)
            {
                await dbContext.UpdateVoteMetadataAsync(postId);
            }

            foreach (var postId in commentSnapshot)
            {
                // Recount comments for accuracy after batching
                var commentCount = await dbContext.Comments.CountAsync(c => c.PostId == postId, ct);
                await dbContext.PostMetadatas
                    .Where(m => m.PostId == postId)
                    .ExecuteUpdateAsync(s => s.SetProperty(m => m.CommentCount, commentCount), ct);
            }

            foreach (var postId in newPostSnapshot)
            {
                var exists = await dbContext.PostMetadatas.AnyAsync(m => m.PostId == postId, ct);
                if (!exists)
                {
                    dbContext.PostMetadatas.Add(new Data.Entities.PostMetadata(postId));
                    await dbContext.SaveChangesAsync(ct);
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Shutting down — expected
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error flushing metadata updates");
        }
    }
}
