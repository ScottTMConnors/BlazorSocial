namespace BlazorSocial.Data.BackgroundJobs;

public abstract record PostEvent;
public record VoteRecorded(PostId PostId) : PostEvent;
public record CommentAdded(PostId PostId) : PostEvent;
public record ViewRecorded(PostId PostId, string? IpAddress, UserId? UserId) : PostEvent;
public record PostCreated(PostId PostId) : PostEvent;
