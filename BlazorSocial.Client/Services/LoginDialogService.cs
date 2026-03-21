using BlazorSocial.Client.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace BlazorSocial.Client.Services;

public class LoginDialogService(IDialogService dialogService)
{
    public Task ShowAsync() => dialogService.ShowDialogAsync<LoginDialog>(options =>
    {
        options.Modal = true;
        options.Style = "padding: 0 !important; overflow: hidden; border-radius: 16px; min-width: 440px; max-width: 440px; background: #1c1c1c !important; border: none !important; box-shadow: 0 16px 48px rgba(0,0,0,0.5);";
        options.Footer.PrimaryAction.Visible = false;
        options.Footer.SecondaryAction.Visible = false;
    });
}
