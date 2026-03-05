using BlazorSocial.Data;
using BlazorSocial.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace BlazorSocial.Components.Account;

internal sealed class IdentityUserAccessor(
    UserManager<SocialUser> userManager,
    IdentityRedirectManager redirectManager)
{
    public async Task<SocialUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            redirectManager.RedirectToWithStatus("Account/InvalidUser",
                $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
        }

        return user;
    }
}