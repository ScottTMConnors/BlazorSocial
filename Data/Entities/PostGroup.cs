using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    [Keyless]
    public class PostGroup {
        [ForeignKey("Post")]
        public string PostId { get; set; }
        [ForeignKey("Group")]
        public string GroupId { get; set; }
        public Post? Post { get; set; }
        public Group? Group { get; set; }
    }
}
