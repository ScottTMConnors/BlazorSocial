namespace BlazorSocial.Shared.Models;

public record ViewPostDto
{
    public PostId PostId { get; init; } = PostId.Empty;
    public string Title { get; init; } = "";
    public string Content { get; init; } = "";
    public DateTime PostDate { get; init; }
    public string PostType { get; init; } = "";
    public string AuthorName { get; init; } = "";
    public int Score { get; set; } = 0;
    public int CommentCount { get; init; } = 0;
    public int CurrentUserVote { get; set; } = 0;
}
