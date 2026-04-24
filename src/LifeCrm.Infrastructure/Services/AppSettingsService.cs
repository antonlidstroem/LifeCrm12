using LifeCrm.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LifeCrm.Infrastructure.Services;

public class AppSettingsService : IAppSettings
{
    private readonly IConfiguration _config;
    public AppSettingsService(IConfiguration config) { _config = config; }

    public string AppBaseUrl => (_config["AppBaseUrl"] ?? string.Empty).TrimEnd('/');

    public string JwtSecretKey => _config.GetSection("Jwt")["SecretKey"]
        ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");

    /// <summary>
    /// Base64-encoded 32-byte AES-256 key for PII field encryption.
    /// Production: inject via environment variable or Azure Key Vault reference.
    /// Generate: Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
    /// </summary>
    public string EncryptionKey => _config["Encryption:FieldKey"]
        ?? throw new InvalidOperationException(
            "Encryption:FieldKey is not configured. " +
            "Generate with: Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))");

    public EmailSettingsDto DefaultEmailSettings
    {
        get
        {
            var s = _config.GetSection("Email");
            return new EmailSettingsDto
            {
                Host = s["Host"] ?? "localhost",
                Port = int.TryParse(s["Port"], out var p) ? p : 587,
                Username = s["Username"] ?? string.Empty,
                Password = s["Password"] ?? string.Empty,
                FromEmail = s["FromEmail"] ?? "noreply@lifecrm.se",
                FromName = s["FromName"] ?? "LifeCrm",
                UseSsl = bool.TryParse(s["UseSsl"], out var ssl) ? ssl : true,
                DryRun = bool.TryParse(s["DryRun"], out var dryRun) ? dryRun : false
            };
        }
    }
}