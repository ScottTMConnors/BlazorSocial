using BlazorSocial.Auth.Contracts;
using BlazorSocial.Components;
using BlazorSocial.Proxy;
using BlazorSocial.ServiceDefaults;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using Refit;
using System.Text;
using Yarp.ReverseProxy.Forwarder;
using BlazorSocialClient = BlazorSocial.Client._Imports;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

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

builder.Services.AddCascadingAuthenticationState();

var jwtKey = builder.Configuration["Jwt:SigningKey"] ?? "development-signing-key-placeholder";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = "blazorsocial-auth",
            ValidateAudience = true,
            ValidAudience = "blazorsocial-api"
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRefitClient<IAuthApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https+http://auth"));

builder.Services.AddHttpForwarder();
builder.Services.AddHttpClient("AuthProxy");
builder.Services.AddHttpClient("ApiProxy");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorSocialClient).Assembly);

// Proxy /auth/* to the standalone Auth service
app.Map("/auth/{**catch-all}", async (IHttpForwarder forwarder, IHttpMessageHandlerFactory handlerFactory, HttpContext context) =>
{
    using var invoker = new HttpMessageInvoker(handlerFactory.CreateHandler("AuthProxy"), disposeHandler: false);
    await forwarder.SendAsync(context, "https+http://auth", invoker, ForwarderRequestConfig.Empty);
});

// Proxy /api/* to the standalone Api service with user ID injection
app.Map("/api/{**catch-all}", async (IHttpForwarder forwarder, IHttpMessageHandlerFactory handlerFactory, HttpContext context) =>
{
    using var invoker = new HttpMessageInvoker(handlerFactory.CreateHandler("ApiProxy"), disposeHandler: false);
    await forwarder.SendAsync(context, "https+http://api", invoker, ForwarderRequestConfig.Empty, new AddUserIdTransformer());
});

app.MapDefaultEndpoints();

app.Run();
