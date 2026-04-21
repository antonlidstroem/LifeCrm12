using System.Security.Cryptography;
using System.Text;
using LifeCrm.Core.Interfaces;

namespace LifeCrm.Infrastructure.Services;

public class UnsubscribeTokenService : IUnsubscribeTokenService
{
    private readonly IAppSettings _appSettings;
    public UnsubscribeTokenService(IAppSettings appSettings) => _appSettings = appSettings;

    public string GenerateToken(Guid contactId, Guid organizationId)
    {
        var payload = Encoding.UTF8.GetBytes($"{contactId}:{organizationId}");
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_appSettings.JwtSecretKey));
        var sig = hmac.ComputeHash(payload);
        return $"{ToBase64Url(payload)}.{ToBase64Url(sig)}";
    }

    private static string ToBase64Url(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
