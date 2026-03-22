using System.Security.Claims;
using BlazorSocial.Data;

namespace BlazorSocial.Api.Extensions;

public static class HttpContextExtensions
{
    extension(HttpContext httpContext)
    {
        public UserId? GetCurrentUserId()
        {
            var user = httpContext.User;
            var isAuthenticated = user.Identity?.IsAuthenticated ?? false;

            if (!isAuthenticated)
            {
                return null;
            }

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim is null)
            {
                return null;
            }

            var userId = UserId.Parse(userIdClaim);
            return userId;
        }
    }
}
