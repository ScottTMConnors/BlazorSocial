using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    [Keyless]
    public class View {
        public string PostId { get; set; }
        public string UserId { get; set; }
        public DateTime ViewDate { get; set; }
        public int TimesViewed { get; set; }
        public Post? Post { get; set; }
        [NotMapped]
        public ApplicationUser? User { get; set; }
    }
}
