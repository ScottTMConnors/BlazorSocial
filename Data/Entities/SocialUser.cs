using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    public class SocialUser {
        [Key]
        public string UserId { get; set; }
        public string UserName { get; set; }
        [NotMapped]
        public ApplicationUser? User { get; set; }
    }
}
