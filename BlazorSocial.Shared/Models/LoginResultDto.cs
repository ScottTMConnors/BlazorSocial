namespace BlazorSocial.Shared.Models;

public record LoginResultDto(bool Succeeded, string? Error = null);
