namespace BlazorSocial.Shared.Models;

public record CreateCommentDto
{
    public string Content { get; init; } = "";
}
