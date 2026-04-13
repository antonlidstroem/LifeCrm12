using System.Security.Cryptography;
using System.Text;
using LifeCrm.Core.Interfaces;

namespace LifeCrm.Infrastructure.Services;

/// <summary>
/// Generates HMAC-SHA256 signed unsubscribe tokens in the format expected by UnsubscribeController.
/// Format: base64url(payload) + "." + base64url(hmac)
/// Payload: "{contactId}:{orgId}"
/// </summary>
public class UnsubscribeTokenService : IUnsubscribeTokenService
{
    private readonly IAppSettings _appSettings;
    public UnsubscribeTokenService(IAppSettings appSettings) { _appSettings = appSettings; }

    public string GenerateToken(Guid contactId, Guid organizationId)
    {
        var payload     = $"{contactId}:{organizationId}";
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac  = new HMACSHA256(Encoding.UTF8.GetBytes(_appSettings.JwtSecretKey));
        var sigBytes    = hmac.ComputeHash(payloadBytes);

        var payloadB64  = ToBase64Url(payloadBytes);
        var sigB64      = ToBase64Url(sigBytes);

        return $"{payloadB64}.{sigB64}";
    }

    private static string ToBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
