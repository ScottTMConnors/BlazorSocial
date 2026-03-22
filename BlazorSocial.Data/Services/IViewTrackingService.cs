namespace BlazorSocial.Data.Services;

public interface IViewTrackingService
{
    Task RecordViewAsync(PostId postId, string? ipAddress, UserId? userId, CancellationToken ct);
}
