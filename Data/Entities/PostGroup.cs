using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    public class PostGroup {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Post")]
        public string PostId { get; set; }
        [ForeignKey("Group")]
        public string GroupId { get; set; }
        public Post? Post { get; set; }
        public Group? Group { get; set; }
    }
}
