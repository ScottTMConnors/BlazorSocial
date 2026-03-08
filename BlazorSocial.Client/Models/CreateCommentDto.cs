namespace BlazorSocial.Client.Models;

public record CreateCommentDto
{
    public string Content { get; init; } = "";
}
