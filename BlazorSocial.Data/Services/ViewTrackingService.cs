using BlazorSocial.Data.BackgroundJobs;

namespace BlazorSocial.Data.Services;

public class ViewTrackingService(ViewEventQueue queue) : IViewTrackingService
{
    public Task RecordViewAsync(PostId postId, string? ipAddress, UserId? userId, CancellationToken ct)
    {
        // Just enqueue — no DB write on the hot path
        queue.Enqueue(new ViewRecorded(postId, ipAddress, userId));
        return Task.CompletedTask;
    }
}
