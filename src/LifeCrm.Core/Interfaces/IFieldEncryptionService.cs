namespace LifeCrm.Core.Interfaces;

public interface IFieldEncryptionService
{
    /// <summary>AES-256-CBC encrypt. Returns Base64 string with prepended IV.</summary>
    string Encrypt(string plaintext);

    /// <summary>Decrypt value produced by Encrypt.</summary>
    string Decrypt(string ciphertext);

    /// <summary>
    /// HMAC-SHA256 deterministic hash of lowercased input.
    /// Used for indexed lookups on encrypted columns (e.g. EmailHash).
    /// </summary>
    string Hash(string plaintext);
}