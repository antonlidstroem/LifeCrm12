namespace LifeCrm.Application.Common.DTOs;
public record LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string OrganizationId { get; init; } = string.Empty;
    public string OrganizationName { get; init; } = string.Empty;
}
