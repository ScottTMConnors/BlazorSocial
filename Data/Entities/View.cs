using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    [PrimaryKey(nameof(PostId), nameof(UserId))]
    public class View {
        [ForeignKey("Post")]
        public string PostId { get; set; }
        [ForeignKey("SocialUser")]
        public string UserId { get; set; }
        public DateTime ViewDate { get; set; }
        public int TimesViewed { get; set; }
        public Post? Post { get; set; }
        public SocialUser? User { get; set; }
    }
}
