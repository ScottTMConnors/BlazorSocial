using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    public class Post {
        [Key]
        public string Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int? PostTypeID { get; set; }
        public string? AuthorID { get; set; }
        public DateTime? PostDate { get; set; }
        public PostType? PostType { get; set; }
        [NotMapped]
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
