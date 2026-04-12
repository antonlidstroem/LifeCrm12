using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LifeCrm.Core.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace LifeCrm.Infrastructure.Services;

public class UnsubscribeTokenService : IUnsubscribeTokenService
{
    private readonly IAppSettings _appSettings;
    public UnsubscribeTokenService(IAppSettings appSettings) { _appSettings = appSettings; }

    public string GenerateToken(Guid contactId, Guid organizationId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JwtSecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
        var claims = new[]
        {
            new Claim("contactId", contactId.ToString()),
            new Claim("orgId",     organizationId.ToString()),
            new Claim("purpose",   "unsubscribe")
        };
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
