using Microsoft.AspNetCore.Identity;

namespace BlazorSocial.Auth;

public class AuthUser : IdentityUser<UserId>
{
    public AuthUser() { Id = UserId.New(); }
    public string DisplayName { get; set; } = "";
}
