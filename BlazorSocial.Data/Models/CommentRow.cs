namespace BlazorSocial.Data.Models;

/// <summary>
/// Internal projection used by Data layer services. Mapped to CommentDto by the Api layer.
/// </summary>
public record CommentRow
{
    public string AuthorName { get; init; } = "";
    public DateTime? PostDate { get; init; }
    public string Content { get; init; } = "";
}
