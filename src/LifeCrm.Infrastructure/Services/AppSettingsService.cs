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

    public string AppBaseUrl  => (_config["AppBaseUrl"]        ?? string.Empty).TrimEnd('/');
    public string JwtSecretKey => _config.GetSection("Jwt")["SecretKey"]
        ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
}
