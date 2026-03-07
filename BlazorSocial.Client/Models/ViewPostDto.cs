namespace BlazorSocial.Client.Models;

public record ViewPostDto
{
    public Guid PostId { get; init; }
    public string Title { get; init; } = "";
    public string Content { get; init; } = "";
    public DateTime PostDate { get; init; }
    public string PostType { get; init; } = "";
    public string AuthorName { get; init; } = "";
}
