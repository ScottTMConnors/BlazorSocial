using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlazorSocial.Auth.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using AuthR = BlazorSocial.Auth.Contracts.AuthRoutes;

namespace BlazorSocial.Auth.Extensions;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(AuthR.Login,
            async Task<IResult> (LoginRequestDto request,
                SignInManager<AuthUser> signInManager,
                UserManager<AuthUser> userManager,
                IConfiguration config,
                CancellationToken ct) =>
            {
                var result = await signInManager.PasswordSignInAsync(
                    request.Email, request.Password, isPersistent: false, lockoutOnFailure: false);

                if (!result.Succeeded)
                    return Results.Unauthorized();

                var user = await userManager.FindByEmailAsync(request.Email);
                if (user is null) return Results.Unauthorized();

                var token = GenerateToken(user, config);
                return Results.Ok(token);
            });

        endpoints.MapPost(AuthR.Register,
            async Task<IResult> (RegisterRequestDto request,
                UserManager<AuthUser> userManager,
                IConfiguration config,
                CancellationToken ct) =>
            {
                var user = new AuthUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    DisplayName = request.DisplayName,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                    return Results.BadRequest(result.Errors);

                var token = GenerateToken(user, config);
                return Results.Ok(token);
            });

        return endpoints;
    }

    private static TokenResponseDto GenerateToken(AuthUser user, IConfiguration config)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SigningKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTimeOffset.UtcNow.AddDays(7);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim("display_name", user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: "blazorsocial-auth",
            audience: "blazorsocial-api",
            claims: claims,
            expires: expires.UtcDateTime,
            signingCredentials: creds);

        return new TokenResponseDto(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
