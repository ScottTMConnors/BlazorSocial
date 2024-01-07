using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    [PrimaryKey(nameof(PostId), nameof(UserId))]
    public class Vote {
        [ForeignKey("Post")]
        public string PostId { get; set; }
        [ForeignKey("SocialUser")]
        public string UserId { get; set; }
        public bool IsUpvote { get; set; } = false;
        public DateTime? VoteDate { get; set; }
        public Post? Post { get; set; }
        public SocialUser? User { get; set; }
        public bool IsActive { get; set; } = false;
    }
}
