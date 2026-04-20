// src/LifeCrm.Infrastructure/Encryption/EncryptedStringConverter.cs
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LifeCrm.Infrastructure.Encryption;

/// <summary>
/// EF Core Value Converter using ASP.NET Core Data Protection API.
/// Registered as a singleton — IDataProtector is thread-safe.
/// Fields using this converter cannot be filtered in SQL — use EmailHash for lookups.
/// </summary>
public class EncryptedStringConverter : ValueConverter<string?, string?>
{
    public EncryptedStringConverter(IDataProtector protector)
        : base(
            plaintext => plaintext == null ? null : protector.Protect(plaintext),
            ciphertext => ciphertext == null ? null : protector.Unprotect(ciphertext))
    { }
}