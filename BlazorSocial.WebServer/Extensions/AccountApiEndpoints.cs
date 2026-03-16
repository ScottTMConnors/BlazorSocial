using BlazorSocial.Data.Entities;
using BlazorSocial.Shared.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace BlazorSocial.Extensions;

public static class AccountApiEndpoints
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder MapAccountApiEndpoints()
        {
            endpoints.MapPost(ApiRoute.Templates.Login, async (
                LoginRequestDto request,
                SignInManager<SocialUser> signInManager) =>
            {
                var result = await signInManager.PasswordSignInAsync(
                    request.Email, request.Password, request.RememberMe, false);

                if (result.Succeeded)
                {
                    return Results.Ok(new LoginResultDto(true));
                }

                if (result.IsLockedOut)
                {
                    return Results.Ok(new LoginResultDto(false, "Your account has been locked out."));
                }

                if (result.RequiresTwoFactor)
                {
                    return Results.Ok(new LoginResultDto(false, "Two-factor authentication is required."));
                }

                return Results.Ok(new LoginResultDto(false, "Invalid email or password."));
            });

            endpoints.MapGet(ApiRoute.Templates.ExternalLogin, (
                HttpContext context,
                [FromServices] SignInManager<SocialUser> signInManager,
                [FromQuery] string provider,
                [FromQuery] string? returnUrl) =>
            {
                IEnumerable<KeyValuePair<string, StringValues>> query =
                [
                    new("ReturnUrl", returnUrl ?? "/"),
                    new("Action", "LoginCallback")
                ];

                var redirectUrl = UriHelper.BuildRelative(
                    context.Request.PathBase,
                    "/Account/ExternalLogin",
                    QueryString.Create(query));

                var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                return TypedResults.Challenge(properties, [provider]);
            });

            return endpoints;
        }
    }
}