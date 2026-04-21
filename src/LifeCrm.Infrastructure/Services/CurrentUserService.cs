using System.Security.Claims;
using LifeCrm.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LifeCrm.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value is { } s && Guid.TryParse(s, out var g) ? g : null;
    public Guid? OrganizationId => User?.FindFirst("org_id")?.Value is { } s && Guid.TryParse(s, out var g) ? g : null;
    public string? UserRole => User?.FindFirst(ClaimTypes.Role)?.Value;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
