using BlazorSocial.Data.Entities;

namespace BlazorSocial.Data.ComponentObjects;

public record ViewPost(Post post)
{
    public PostId PostId { get; set; } = post.Id;
    public string Title { get; set; } = post.Title;
    public string Content { get; set; } = post.Content;
    public DateTime PostDate { get; set; } = post.PostDate;
    public string AuthorName { get; set; } = post.Author?.UserName ?? "Unknown";
}