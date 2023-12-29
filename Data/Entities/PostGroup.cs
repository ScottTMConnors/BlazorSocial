using Microsoft.EntityFrameworkCore;

namespace BlazorSocial.Data.Entities {
    [Keyless]
    public class PostGroup {
        public string PostId { get; set; }
        public string GroupId { get; set; }
        public Post? Post { get; set; }
        public Group? Group { get; set; }
    }
}
