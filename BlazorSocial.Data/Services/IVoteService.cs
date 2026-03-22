namespace BlazorSocial.Data.Services;

public interface IVoteService
{
    Task VoteAsync(PostId postId, UserId userId, bool isUpvote, CancellationToken ct);
}
