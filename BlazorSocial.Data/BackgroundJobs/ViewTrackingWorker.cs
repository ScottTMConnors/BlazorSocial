using BlazorSocial.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlazorSocial.Data.BackgroundJobs;

public sealed class ViewTrackingWorker(
    PostEventQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<ViewTrackingWorker> logger) : BackgroundService
{
    private const int FlushIntervalSeconds = 5;
    private const int FlushBatchSize = 500;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pending = new List<ViewRecorded>(FlushBatchSize);
        using var flushTimer = new PeriodicTimer(TimeSpan.FromSeconds(FlushIntervalSeconds));

        var readTask = ReadViewEventsAsync(queue, pending, stoppingToken);
        var flushTask = FlushPeriodicallyAsync(flushTimer, pending, stoppingToken);

        await Task.WhenAll(readTask, flushTask);
    }

    private static async Task ReadViewEventsAsync(
        PostEventQueue queue,
        List<ViewRecorded> pending,
        CancellationToken ct)
    {
        await foreach (var evt in queue.ReadAllAsync(ct))
        {
            if (evt is ViewRecorded view)
            {
                pending.Add(view);
            }
        }
    }

    private async Task FlushPeriodicallyAsync(
        PeriodicTimer timer,
        List<ViewRecorded> pending,
        CancellationToken ct)
    {
        while (await timer.WaitForNextTickAsync(ct))
        {
            await FlushAsync(pending, ct);
        }
    }

    private async Task FlushAsync(List<ViewRecorded> pending, CancellationToken ct)
    {
        if (pending.Count == 0) return;

        var snapshot = pending.ToList();
        pending.Clear();

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ContentDbContext>>();
            await using var dbContext = await dbFactory.CreateDbContextAsync(ct);

            var views = snapshot.Select(v => new View
            {
                PostId = v.PostId,
                UserId = v.UserId,
                ViewDateTime = DateTime.Now
            }).ToList();

            dbContext.Views.AddRange(views);
            await dbContext.SaveChangesAsync(ct);

            // Update view counts per post
            foreach (var postId in snapshot.Select(v => v.PostId).Distinct())
            {
                var count = snapshot.Count(v => v.PostId == postId);
                var updated = await dbContext.PostMetadatas
                    .Where(m => m.PostId == postId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(m => m.ViewCount, m => m.ViewCount + count), ct);

                if (updated == 0)
                {
                    dbContext.PostMetadatas.Add(new Entities.PostMetadata(postId) { ViewCount = count });
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
            logger.LogError(ex, "Error flushing view tracking data");
        }
    }
}
