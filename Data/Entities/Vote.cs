using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    [Keyless]
    public class Vote {
        public string PostId { get; set; }
        public string UserId { get; set; }
        public bool IsUpvote { get; set; }
        public DateTime? VoteDate { get; set; }
        public Post? Post { get; set; }
        [NotMapped]
        public ApplicationUser? User { get; set; }
    }
}
