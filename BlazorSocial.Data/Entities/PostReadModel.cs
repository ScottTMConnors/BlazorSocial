using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities;

/// <summary>
/// Denormalized read model for the post feed. Populated synchronously on post create
/// and kept up-to-date by background workers. No CurrentUserVote — resolved at query time.
/// </summary>
public class PostReadModel
{
    [Key]
    [ForeignKey("Post")]
    public PostId PostId { get; set; } = PostId.Empty;

    [StringLength(100)]
    public string Title { get; set; } = "";

    [StringLength(3999)]
    public string Content { get; set; } = "";

    public DateTime PostDate { get; set; }

    /// <summary>PostType stored as a string — avoids SmartEnum converter complexity on the read path.</summary>
    [StringLength(50)]
    public string PostType { get; set; } = "";

    /// <summary>Denormalized from SocialUser at creation time.</summary>
    [StringLength(256)]
    public string AuthorName { get; set; } = "";

    public int Upvotes { get; set; }
    public int Downvotes { get; set; }
    public int NetVotes { get; set; }
    public int ViewCount { get; set; }
    public int CommentCount { get; set; }
}
