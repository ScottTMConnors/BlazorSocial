using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public SocialUser? Author { get; set; }
    public PostMetadata PostMetadata { get; set; }

    public PostType PostType { get; set; }

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
