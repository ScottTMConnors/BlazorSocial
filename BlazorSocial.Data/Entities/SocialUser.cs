using System.ComponentModel.DataAnnotations;

namespace BlazorSocial.Data.Entities;

public class SocialUser : BaseEntity<UserId>
{
    public SocialUser() { }

    public SocialUser(UserId id)
    {
        Id = id;
    }

    [StringLength(30)]
    public string DisplayName { get; set; } = "";

    public string? UserName { get; set; }

    public string? Email { get; set; }
}
