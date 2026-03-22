using Refit;
using Routes = BlazorSocial.Auth.Contracts.AuthRoutes;

namespace BlazorSocial.Auth.Contracts;

public interface IAuthApi
{
    [Post(Routes.Login)]
    Task<TokenResponseDto> LoginAsync([Body] LoginRequestDto request, CancellationToken ct = default);

    [Post(Routes.Register)]
    Task<TokenResponseDto> RegisterAsync([Body] RegisterRequestDto request, CancellationToken ct = default);
}
