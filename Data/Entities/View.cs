using System.ComponentModel.DataAnnotations.Schema;
using BlazorSocial.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorSocial.Data.Entities;

[PrimaryKey(nameof(PostId), nameof(UserId))]
public record View
{
    [ForeignKey("Post")] public PostId PostId { get; set; } = null!;

    [ForeignKey("User")] public UserId? UserId { get; set; }

    public DateTime ViewDateTime { get; set; }

    [NotMapped] public Post? Post { get; set; }

    [NotMapped] public SocialUser? User { get; set; }
}