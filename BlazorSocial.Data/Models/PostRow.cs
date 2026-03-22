namespace BlazorSocial.Data.Models;

/// <summary>
/// Internal projection used by Data layer services. Mapped to ViewPostDto by the Api layer.
/// </summary>
public record PostRow
{
    public PostId PostId { get; init; } = PostId.Empty;
    public string Title { get; init; } = "";
    public string Content { get; init; } = "";
    public DateTime PostDate { get; init; }
    public string PostType { get; init; } = "";
    public string AuthorName { get; init; } = "";
    public int Score { get; set; }
    public int CommentCount { get; init; }
    public int CurrentUserVote { get; set; }
}
