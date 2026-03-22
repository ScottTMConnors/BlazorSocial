using BlazorSocial.Auth.Contracts;
using BlazorSocial.Client.Services;
using BlazorSocial.Shared.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddRefitClient<IPostsApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddRefitClient<IAuthApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddFluentUIComponents(options =>
{
    options.MarkupSanitized.SanitizeInlineStyle = value =>
    {
        if (value.Contains("url(", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("expression(", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("javascript:", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("@import", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("-moz-binding", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"The provided CSS inline style contains potentially unsafe content: {value}");
        }

        return value;
    };
});
builder.Services.AddScoped<LoginDialogService>();
await builder.Build().RunAsync();