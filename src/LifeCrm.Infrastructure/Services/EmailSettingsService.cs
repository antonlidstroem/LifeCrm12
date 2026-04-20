using System.Text.Json;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.Services;

/// <summary>
/// Persists outgoing email (SMTP) configuration as a JSON blob in the AppSettings table
/// under the key "Email:Settings". Falls back to appsettings.json values when not set.
/// Registered as Singleton so the in-memory cache is shared across all requests.
/// </summary>
public class EmailSettingsService : IEmailSettingsService
{
    private const string Key = "Email:Settings";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAppSettings         _appConfig;
    private readonly ILogger<EmailSettingsService> _logger;

    // Thread-safe in-memory cache with 60-second TTL
    private volatile EmailSettingsDto? _cached;
    private DateTimeOffset _cacheExpiry = DateTimeOffset.MinValue;
    private readonly object _cacheLock  = new();

    public EmailSettingsService(
        IServiceScopeFactory scopeFactory,
        IAppSettings appConfig,
        ILogger<EmailSettingsService> logger)
    {
        _scopeFactory = scopeFactory;
        _appConfig    = appConfig;
        _logger       = logger;
    }

    public async Task<EmailSettingsDto> GetAsync(CancellationToken ct = default)
    {
        // Return cached value if still fresh
        lock (_cacheLock)
        {
            if (_cached is not null && DateTimeOffset.UtcNow < _cacheExpiry)
                return _cached;
        }

        var fromDb = JsonSerializer.Deserialize<EmailSettingsDto>(row.Value, ...);
        return fromDb with
        {
            Password = string.IsNullOrEmpty(fromDb.Password)
                ? fromDb.Password
                : _protector.Unprotect(fromDb.Password)
        };

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var row = await db.AppSettings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Key == Key, ct);

            if (row is not null && !string.IsNullOrWhiteSpace(row.Value))
            {
                var fromDb = JsonSerializer.Deserialize<EmailSettingsDto>(row.Value,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (fromDb is not null)
                {
                    lock (_cacheLock)
                    {
                        _cached      = fromDb;
                        _cacheExpiry = DateTimeOffset.UtcNow.AddSeconds(60);
                    }
                    return fromDb;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Could not read email settings from DB. Falling back to appsettings.json.");
        }

        // Fallback: appsettings.json values
        return _appConfig.DefaultEmailSettings;
    }

    public async Task SaveAsync(EmailSettingsDto settings, CancellationToken ct = default)
    {

        var toStore = settings with
        {
            Password = string.IsNullOrEmpty(settings.Password)
        ? settings.Password
        : _protector.Protect(settings.Password)
        };

        var json = JsonSerializer.Serialize(settings);
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var row = await db.AppSettings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Key == Key, ct);

            if (row is null)
            {
                row = new Core.Entities.AppSettings
                {
                    Id    = Guid.NewGuid(),
                    Key   = Key,
                    Value = json
                };
                db.AppSettings.Add(row);
            }
            else
            {
                row.Value = json;
                db.Entry(row).State = EntityState.Modified;
            }

            await db.SaveChangesAsync(ct);

            // Immediately update cache so the next request picks up the new config
            lock (_cacheLock)
            {
                _cached      = settings;
                _cacheExpiry = DateTimeOffset.UtcNow.AddSeconds(60);
            }

            _logger.LogInformation("Email settings updated and cached.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save email settings to DB.");
            throw;
        }
    }
}
