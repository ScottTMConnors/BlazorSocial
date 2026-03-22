namespace BlazorSocial.Auth.Contracts;

public record RegisterRequestDto(string Email, string Password, string DisplayName);
