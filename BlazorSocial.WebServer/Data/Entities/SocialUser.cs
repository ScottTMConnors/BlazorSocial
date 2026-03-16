using System.ComponentModel.DataAnnotations;
using BlazorSocial.Data;
using Microsoft.AspNetCore.Identity;

namespace BlazorSocial.Data.Entities {
    public class SocialUser : IdentityUser<UserId>, IEntity<UserId> {
        public SocialUser()
        {
            Id = UserId.New();
        }

        [StringLength(30)]
        public string DisplayName { get; set; } = "";
    }
}
