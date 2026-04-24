using LifeCrm.Core.Interfaces;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LifeCrm.Infrastructure.Persistence.Converters;

/// <summary>
/// EF Core value converter that transparently encrypts on write and decrypts on read.
/// Applied to PII string columns via entity configuration.
/// NULL values pass through unchanged.
/// </summary>
public class EncryptedConverter : ValueConverter<string?, string?>
{
    public EncryptedConverter(IFieldEncryptionService enc)
        : base(
            // Write path: plaintext → ciphertext
            v => v == null ? null : enc.Encrypt(v),
            // Read path: ciphertext → plaintext
            v => v == null ? null : enc.Decrypt(v))
    { }
}