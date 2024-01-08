using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    public class Comment {
        [Key]
        public string Id { get; set; }
        [ForeignKey("Post")]
        public string PostId { get; set; }
        public Post? Post { get; set; }
        [ForeignKey("SocialUser")]
        public string? AuthorID { get; set; }
        public SocialUser? Author { get; set; }
        [StringLength(1000)]
        public string Content { get; set; }
        public DateTime PostDate { get; set; }
    }
}
