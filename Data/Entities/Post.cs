using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    public class Post {
        [Key]
        public string Id { get; set; }
        [StringLength(100)]
        public string? Title { get; set; }
        [StringLength(3999)]
        public string? Content { get; set; }
        public int? PostTypeID { get; set; }
        [ForeignKey("SocialUser")]
        public string? AuthorID { get; set; }
        public DateTime? PostDate { get; set; }
        public PostType? PostType { get; set; }
        public SocialUser? Author { get; set; }
        public PostMetadata? PostMetadata { get; set; }
        [NotMapped]
        public IEnumerable<Group>? Groups { get; set; }
        [NotMapped]
        public IEnumerable<Vote>? Votes { get; set; }
        [NotMapped]
        public IEnumerable<View>? Views { get; set; }
    }
}
