using BlazorSocial.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    [PrimaryKey(nameof(PostId), nameof(UserId))]
    public class Vote {
        [ForeignKey("Post")]
        public PostId PostId { get; set; } = null!;
        [ForeignKey("User")]
        public UserId UserId { get; set; } = null!;
        public bool IsUpvote { get; set; }
        public DateTime? VoteDate { get; set; }
        public Post? Post { get; set; }
        public SocialUser? User { get; set; }
        public bool IsActive { get; set; }
    }
}
