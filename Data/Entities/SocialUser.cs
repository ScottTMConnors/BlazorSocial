﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorSocial.Data.Entities {
    public class SocialUser {
        [Key]
        public string UserId { get; set; }
        [StringLength(30)]
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        [NotMapped]
        public ApplicationUser? User { get; set; }
    }
}
