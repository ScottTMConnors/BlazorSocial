using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BlazorSocial.Data;

namespace BlazorSocial.Data.Entities {
    public class Comment : BaseEntity<CommentId> {
        [ForeignKey("Post")]
        public PostId PostId { get; set; } = null!;
        public Post? Post { get; set; }
        [ForeignKey("Author")]
        public UserId? AuthorID { get; set; }
        public SocialUser? Author { get; set; }
        [StringLength(1000)]
        public string Content { get; set; }
        public DateTime PostDate { get; set; }
    }
}
