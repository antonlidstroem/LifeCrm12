using System.Text.Json;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.Services;

/// <summary>
/// Stores outgoing email configuration as a JSON blob in the AppSettings table
/// under the key "Email:Settings". Falls back to appsettings.json values if not set.
/// </summary>
public class EmailSettingsService : IEmailSettingsService
{
    private const string Key = "Email:Settings";
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAppSettings _appConfig;
    private readonly ILogger<EmailSettingsService> _logger;

    // Simple in-memory cache with 60-second TTL
    private EmailSettingsDto? _cached;
    private DateTimeOffset _cacheExpiry = DateTimeOffset.MinValue;

    public EmailSettingsService(IServiceScopeFactory scopeFactory, IAppSettings appConfig,
        ILogger<EmailSettingsService> logger)
    {
        _scopeFactory = scopeFactory;
        _appConfig    = appConfig;
        _logger       = logger;
    }

    public async Task<EmailSettingsDto> GetAsync(CancellationToken ct = default)
    {
        if (_cached is not null && DateTimeOffset.UtcNow < _cacheExpiry)
            return _cached;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var row = await db.AppSettings.IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Key == Key, ct);

            if (row is not null && !string.IsNullOrWhiteSpace(row.Value))
            {
                var settings = JsonSerializer.Deserialize<EmailSettingsDto>(row.Value);
                if (settings is not null)
                {
                    _cached      = settings;
                    _cacheExpiry = DateTimeOffset.UtcNow.AddSeconds(60);
                    return settings;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read email settings from DB. Using config file fallback.");
        }

        // Fallback: read from IConfiguration via IAppSettings
        return _appConfig.DefaultEmailSettings;
    }

    public async Task SaveAsync(EmailSettingsDto settings, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(settings);
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var row = await db.AppSettings.IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Key == Key, ct);
            if (row is null)
            {
                row = new Core.Entities.AppSettings { Id = Guid.NewGuid(), Key = Key, Value = json };
                db.AppSettings.Add(row);
            }
            else
            {
                row.Value = json;
                db.Entry(row).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            await db.SaveChangesAsync(ct);

            // Bust cache
            _cached      = settings;
            _cacheExpiry = DateTimeOffset.UtcNow.AddSeconds(60);
            _logger.LogInformation("Email settings updated by admin.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save email settings.");
            throw;
        }
    }
}
