namespace BlazorSocial.Client.Models;

public record CommentDto
{
    public string AuthorName { get; init; } = "";
    public DateTime? PostDate { get; init; }
    public string Content { get; init; } = "";
}
