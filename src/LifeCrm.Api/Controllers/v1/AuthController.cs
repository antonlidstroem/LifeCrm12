using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LifeCrm.Api.Controllers.v1;

[AllowAnonymous]
public class AuthController : ApiControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db     = db;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await _db.Users
            .IgnoreQueryFilters()
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == req.Email && !u.IsDeleted && u.IsActive, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(ApiResponse.Fail("Invalid email or password."));

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        var token = GenerateToken(user);
        return OkResponse(new LoginResponse
        {
            Token        = token,
            UserId       = user.Id,
            FullName     = user.FullName,
            Email        = user.Email,
            Role         = user.Role.ToString(),
            OrgId        = user.OrganizationId,
            OrgName      = user.Organization?.Name ?? string.Empty,
            ExpiresAt    = DateTimeOffset.UtcNow.AddHours(
                double.TryParse(_config["Jwt:ExpiryHours"], out var h) ? h : 8)
        });
    }

    private string GenerateToken(ApplicationUser user)
    {
        var secret   = _config["Jwt:SecretKey"]!;
        var issuer   = _config["Jwt:Issuer"]   ?? "LifeCrm";
        var audience = _config["Jwt:Audience"] ?? "LifeCrmUsers";
        var hours    = double.TryParse(_config["Jwt:ExpiryHours"], out var h) ? h : 8;
        var key      = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds    = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Name,           user.FullName),
            new Claim(ClaimTypes.Role,           user.Role.ToString()),
            new Claim("org_id",                  user.OrganizationId.ToString())
        };

        var jwt = new JwtSecurityToken(
            issuer, audience, claims,
            expires: DateTime.UtcNow.AddHours(hours),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}

public record LoginRequest
{
    [Required][EmailAddress] public string Email    { get; init; } = string.Empty;
    [Required]               public string Password { get; init; } = string.Empty;
}

public record LoginResponse
{
    public string Token     { get; init; } = string.Empty;
    public Guid   UserId    { get; init; }
    public string FullName  { get; init; } = string.Empty;
    public string Email     { get; init; } = string.Empty;
    public string Role      { get; init; } = string.Empty;
    public Guid   OrgId     { get; init; }
    public string OrgName   { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
}
