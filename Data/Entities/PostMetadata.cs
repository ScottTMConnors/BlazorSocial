using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    public class PostMetadata {
        [Key]
        [ForeignKey("Post")]
        public string PostId { get; set; }
        public Post? Post { get; set; }
        public int? Upvotes { get; set; }
        public int? Downvotes { get; set; }
        public int? TotalVotes { get; set; }
        public int? NetVotes { get; set; }
        public int? ViewCount { get; set; }
        [NotMapped]
        public IEnumerable<Vote>? Votes { get; set; }
        [NotMapped]
        public IEnumerable<View>? Views { get; set; }
    }
}
