using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlazorSocial.Client.Models;

namespace BlazorSocial.Data.Entities;

public class Post : BaseEntity<PostId>
{
    public Post(string title, string content, UserId authorId, DateTime postDate, PostType postType)
    {
        Title = title;
        Content = content;
        AuthorId = authorId;
        PostDate = postDate;
        PostType = postType;
        PostMetadata = new PostMetadata(Id);
    }

    [StringLength(100)] public string Title { get; set; }

    [StringLength(3999)] public string Content { get; set; }

    [ForeignKey(nameof(Author))] public UserId AuthorId { get; set; }

    public DateTime PostDate { get; set; }

    //public PostType? PostType { get; set; }
    public SocialUser? Author { get; set; }
    public PostMetadata PostMetadata { get; set; }

    public PostType PostType { get; set; }

    [NotMapped] public IEnumerable<Group>? Groups { get; set; }

    [NotMapped] public IEnumerable<Vote>? Votes { get; set; }

    [NotMapped] public IEnumerable<View>? Views { get; set; }
}

public static class PostExtensions
{
    extension(Post post)
    {
        public ViewPostDto ToViewPostDto() =>
            new()
            {
                PostId = post.Id.Value,
                Title = post.Title,
                Content = post.Content,
                PostDate = post.PostDate,
                PostType = post.PostType.ToString(),
                AuthorName = post.Author?.UserName ?? "Unknown"
            };
    }
}