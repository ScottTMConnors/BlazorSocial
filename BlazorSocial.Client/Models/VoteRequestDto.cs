namespace BlazorSocial.Client.Models;

public record VoteRequestDto
{
    public bool IsUpvote { get; init; }
}
