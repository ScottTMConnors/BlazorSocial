namespace BlazorSocial.Data.Models;

/// <summary>
/// Internal projection used by Data layer services. Mapped to PostDetailsDto by the Api layer.
/// </summary>
public record PostDetailRow
{
    public PostId PostId { get; init; } = PostId.Empty;
    public string Title { get; init; } = "";
    public string Content { get; init; } = "";
    public DateTime PostDate { get; init; }
    public int ViewCount { get; init; }
    public int NetVotes { get; init; }
    public int Upvotes { get; init; }
    public int Downvotes { get; init; }
    public bool IsAuthenticated { get; init; }
    public int CurrentUserVote { get; init; }
    public int CommentCount { get; init; }
}
