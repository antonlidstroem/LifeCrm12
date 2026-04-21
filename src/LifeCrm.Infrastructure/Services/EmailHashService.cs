using System.Security.Cryptography;
using System.Text;
using LifeCrm.Core.Interfaces;

namespace LifeCrm.Infrastructure.Services;

/// <summary>
/// Produces a deterministic HMAC-SHA256 hash of an email address.
/// Stored in Contact.EmailHash alongside the encrypted Email column,
/// enabling WHERE EmailHash = @hash queries without decrypting every row.
///
/// Uses the same JwtSecretKey as the HMAC signing key — both are application secrets.
/// If the signing key is rotated, EmailHash values must be recomputed via a migration job.
/// </summary>
public class EmailHashService : IEmailHashService
{
    private readonly byte[] _key;

    public EmailHashService(IAppSettings appSettings)
    {
        _key = Encoding.UTF8.GetBytes(appSettings.JwtSecretKey);
    }

    public string Hash(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        using var hmac = new HMACSHA256(_key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
