namespace BlazorSocial.Shared.Models;

public record CreatePostDto
{
    public string Title { get; init; } = "";
    public string Content { get; init; } = "";
}
