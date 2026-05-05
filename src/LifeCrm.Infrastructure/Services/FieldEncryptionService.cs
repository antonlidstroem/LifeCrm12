using System.Security.Cryptography;
using System.Text;
using LifeCrm.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.Services;

/// <summary>
/// AES-256-CBC field encryption with random IV per value.
/// The IV is prepended to the ciphertext so each encrypted value is self-contained.
/// Thread-safe — key bytes are read-only after construction.
/// </summary>
public class AesFieldEncryptionService : IFieldEncryptionService
{
    private readonly byte[] _key;
    private readonly ILogger<AesFieldEncryptionService> _logger;

    public AesFieldEncryptionService(IAppSettings settings, ILogger<AesFieldEncryptionService> logger)
    {
        _logger = logger;
        try
        {
            _key = Convert.FromBase64String(settings.EncryptionKey);
            if (_key.Length != 32)
                throw new InvalidOperationException("EncryptionKey must be exactly 32 bytes (256 bits).");
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("EncryptionKey is not valid Base64.");
        }
    }

    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext)) return plaintext;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV(); // fresh IV for every encryption call

        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertextBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

        // Layout: [16 bytes IV][n bytes ciphertext]
        var result = new byte[aes.IV.Length + ciphertextBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(ciphertextBytes, 0, result, aes.IV.Length, ciphertextBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext)) return ciphertext;

        try
        {
            var fullBytes = Convert.FromBase64String(ciphertext);
            if (fullBytes.Length < 17)
                throw new CryptographicException("Ciphertext too short.");

            var iv = fullBytes[..16];
            var data = fullBytes[16..];

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(data, 0, data.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Field decryption failed — returning empty string to prevent data leak.");
            // Return empty rather than throwing to prevent 500s on partially-migrated data.
            // Log is critical here for ops alerting.
            return string.Empty;
        }
    }

    public string Hash(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext)) return string.Empty;

        // HMAC-SHA256: deterministic, keyed, collision-resistant, not reversible
        using var hmac = new HMACSHA256(_key);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(plaintext.ToLowerInvariant().Trim()));
        return Convert.ToHexString(hashBytes); // 64 hex chars
    }
}