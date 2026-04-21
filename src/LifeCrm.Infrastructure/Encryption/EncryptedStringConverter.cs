using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LifeCrm.Infrastructure.Encryption;

/// <summary>
/// EF Core Value Converter that encrypts/decrypts string properties using the
/// ASP.NET Core Data Protection API.
///
/// Fields using this converter cannot be used in SQL WHERE clauses or indexes.
/// For Contact.Email specifically, use Contact.EmailHash (deterministic HMAC)
/// for indexed lookups.
///
/// Registered as a singleton — IDataProtector is thread-safe.
/// Keys are persisted to the DataProtectionKeys DB table and rotate every 90 days.
/// </summary>
public class EncryptedStringConverter : ValueConverter<string?, string?>
{
    public EncryptedStringConverter(IDataProtector protector)
        : base(
            plaintext  => plaintext == null ? null : protector.Protect(plaintext),
            ciphertext => ciphertext == null ? null : SafeUnprotect(protector, ciphertext))
    {
    }

    /// <summary>
    /// Gracefully handle rows written before encryption was enabled.
    /// If unprotect fails the value is not a valid ciphertext — return null and log
    /// rather than crashing the application on startup.
    /// </summary>
    private static string? SafeUnprotect(IDataProtector protector, string ciphertext)
    {
        try
        {
            return protector.Unprotect(ciphertext);
        }
        catch (Exception)
        {
            // Value was stored before encryption was introduced (plaintext).
            // Return as-is so existing data is not lost. A migration job should
            // re-encrypt these rows — see Infrastructure/Migrations/README.md.
            return ciphertext;
        }
    }
}
