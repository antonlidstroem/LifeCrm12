using LifeCrm.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LifeCrm.Infrastructure.Services;

public class AppSettingsService : IAppSettings
{
    private readonly IConfiguration _config;

    public AppSettingsService(IConfiguration config) => _config = config;

    public string AppBaseUrl   => _config["App:BaseUrl"] ?? "https://localhost:7001";
    public string JwtSecretKey => _config["Jwt:SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey not configured.");

    public EmailSettingsDto DefaultEmailSettings => new()
    {
        Host      = _config["Email:Host"]      ?? string.Empty,
        Port      = int.TryParse(_config["Email:Port"], out var p) ? p : 587,
        Username  = _config["Email:Username"]  ?? string.Empty,
        Password  = _config["Email:Password"]  ?? string.Empty,
        FromEmail = _config["Email:FromEmail"] ?? string.Empty,
        FromName  = _config["Email:FromName"]  ?? "LifeCrm",
        UseSsl    = bool.TryParse(_config["Email:UseSsl"], out var s) && s,
        DryRun    = bool.TryParse(_config["Email:DryRun"],   out var d) && d
    };
}
