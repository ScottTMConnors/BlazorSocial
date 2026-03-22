namespace BlazorSocial.Auth.Contracts;

public record TokenResponseDto(string Token, DateTimeOffset ExpiresAt);
