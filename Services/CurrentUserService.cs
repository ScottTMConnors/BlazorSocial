using System.Security.Claims;
using BlazorSocial.Data.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BlazorSocial.Services;

public class CurrentUserService : IDisposable
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly UserManager<SocialUser> _userManager;

    public CurrentUserService(
        AuthenticationStateProvider authenticationStateProvider,
        UserManager<SocialUser> userManager)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _userManager = userManager;

        _authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    public SocialUser? CurrentUser { get; protected set; }

    public void Dispose()
    {
        _authenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }

    public event Action? OnChange;

    private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        var authState = await task;
        await UpdateCurrentUser(authState.User);
        OnChange?.Invoke();
    }

    private async Task UpdateCurrentUser(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated ?? false)
        {
            var userId = user.FindFirst(c =>
                c.Type == ClaimTypes.NameIdentifier)?.Value;
            CurrentUser = userId is not null
                ? await _userManager.FindByIdAsync(userId)
                : null;
        }
        else
        {
            CurrentUser = null;
        }
    }
}