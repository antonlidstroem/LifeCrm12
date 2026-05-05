namespace LifeCrm.Core.Interfaces;

public interface IAppSettings
{
    string AppBaseUrl { get; }
    string JwtSecretKey { get; }

    /// <summary>
    /// Base64-encoded 32-byte AES key for field-level encryption.
    /// In production: sourced from Azure Key Vault, not appsettings.json.
    /// Generate with: Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
    /// </summary>
    string EncryptionKey { get; }

    EmailSettingsDto DefaultEmailSettings { get; }
}