namespace BlazorSocial.Client.Models;

public record VoteResponseDto
{
    public bool IsUpvote { get; init; }
    public bool IsActive { get; init; }
    public int NetVotes { get; init; }
    public int Upvotes { get; init; }
    public int Downvotes { get; init; }
}
