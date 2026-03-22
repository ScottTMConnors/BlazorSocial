namespace BlazorSocial.Auth.Contracts;

public record LoginResultDto(bool Succeeded, string? Error = null);
