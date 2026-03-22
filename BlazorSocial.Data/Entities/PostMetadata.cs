using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities;

public class PostMetadata
{
    public PostMetadata(PostId postId) => PostId = postId;

    [Key] [ForeignKey("Post")] public PostId PostId { get; set; } = null!;

    public int Upvotes { get; set; }
    public int Downvotes { get; set; }
    public int TotalVotes { get; set; }
    public int NetVotes { get; set; }
    public int ViewCount { get; set; }
    public int CommentCount { get; set; }

    [NotMapped] public IEnumerable<Vote>? Votes { get; set; }

    [NotMapped] public IEnumerable<View>? Views { get; set; }
}
