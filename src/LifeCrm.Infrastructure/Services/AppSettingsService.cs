using LifeCrm.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LifeCrm.Infrastructure.Services;

/// <summary>
/// Bridges IConfiguration (Infrastructure concern) to IAppSettings (Core abstraction).
/// This keeps IConfiguration out of the Application layer entirely.
/// </summary>
public class AppSettingsService : IAppSettings
{
    private readonly IConfiguration _config;
    public AppSettingsService(IConfiguration config) { _config = config; }

    public string AppBaseUrl   => (_config["AppBaseUrl"] ?? string.Empty).TrimEnd('/');
    public string JwtSecretKey => _config.GetSection("Jwt")["SecretKey"]
        ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");

    /// <summary>
    /// Reads email defaults from the "Email" section of appsettings.json.
    /// These are used as a fallback when the admin has not yet saved DB overrides.
    /// </summary>
    public EmailSettingsDto DefaultEmailSettings
    {
        get
        {
            var s = _config.GetSection("Email");
            return new EmailSettingsDto
            {
                Host      = s["Host"]      ?? "localhost",
                Port      = int.TryParse(s["Port"], out var p) ? p : 587,
                Username  = s["Username"]  ?? string.Empty,
                Password  = s["Password"]  ?? string.Empty,
                FromEmail = s["FromEmail"] ?? "noreply@lifecrm.se",
                FromName  = s["FromName"]  ?? "LifeCrm",
                UseSsl    = bool.TryParse(s["UseSsl"],  out var ssl)    ? ssl    : true,
                DryRun    = bool.TryParse(s["DryRun"],  out var dryRun) ? dryRun : false
            };
        }
    }
}
