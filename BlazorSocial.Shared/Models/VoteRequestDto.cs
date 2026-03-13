namespace BlazorSocial.Shared.Models;

public record VoteRequestDto
{
    public bool IsUpvote { get; init; }
}
