using BlazorSocial.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    public class PostGroup {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Post")]
        public PostId PostId { get; set; } = null!;
        [ForeignKey("Group")]
        public GroupId GroupId { get; set; } = null!;
        public Post? Post { get; set; }
        public Group? Group { get; set; }
    }
}
