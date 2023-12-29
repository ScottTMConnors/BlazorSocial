using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    public class Post {
        public string Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int? PostTypeID { get; set; }
        public string? AuthorID { get; set; }
        public DateTime? PostDate { get; set; }
        [NotMapped]
        public int? Upvotes { get; set; }
        [NotMapped]
        public int? Downvotes { get; set; }
        public PostType? PostType { get; set; }
        [NotMapped]
        public ApplicationUser? Author { get; set; }
        [NotMapped]
        public int? TotalVotes { get; set; }
        public int? ViewCount { get; set; }
        [NotMapped]
        public PostMetadata? PostMetadata { get; set; }

        [NotMapped]
        public IEnumerable<Group>? Groups { get; set; }
    }
}
