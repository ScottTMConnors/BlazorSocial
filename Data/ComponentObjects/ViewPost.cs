using BlazorSocial.Data.Entities;

namespace BlazorSocial.Data.ComponentObjects;

public record ViewPost(Post Post)
{
    public PostId PostId { get; set; } = Post.Id;
    public string Title { get; set; } = Post.Title;
    public string Content { get; set; } = Post.Content;
    public DateTime PostDate { get; set; } = Post.PostDate;
    public PostType PostType { get; set; } = Post.PostType;
    public string AuthorName { get; set; } = Post.Author?.UserName ?? "Unknown";
}