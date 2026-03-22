using System.ComponentModel.DataAnnotations.Schema;
using BlazorSocial.Data;

namespace BlazorSocial.Data.Entities;

public class View : BaseEntity<ViewId>
{
    [ForeignKey("Post")] public PostId PostId { get; set; } = null!;

    [ForeignKey("User")] public UserId? UserId { get; set; }

    public DateTime ViewDateTime { get; set; }

    public Post? Post { get; set; }

    public SocialUser? User { get; set; }
}
