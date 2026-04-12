using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection; // Viktigt för IServiceScopeFactory
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.Services;

/// <summary>
/// Persists the SignalR enabled/disabled toggle in the AppSettings table.
/// Registered as Singleton to allow injection into other Singletons (like IActivityNotifier).
/// Uses IServiceScopeFactory to access the scoped AppDbContext.
/// </summary>
public class SignalRSettingsService : ISignalRSettings
{
    private const string Key = "SignalR:Enabled";

    // Eftersom klassen nu är Singleton behövs inte 'static' egentligen, 
    // men vi behåller logiken för trådsäkerhet.
    private volatile bool _cachedValue = true;
    private DateTimeOffset _cacheExpiry = DateTimeOffset.MinValue;
    private readonly object _cacheLock = new();

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SignalRSettingsService> _logger;

    public SignalRSettingsService(IServiceScopeFactory scopeFactory, ILogger<SignalRSettingsService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<bool> GetEnabledAsync(CancellationToken ct = default)
    {
        if (DateTimeOffset.UtcNow < _cacheExpiry) return _cachedValue;

        try
        {
            // Vi skapar ett eget scope för att kunna hämta ut AppDbContext
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var row = await db.AppSettings.IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Key == Key, ct);

            var value = row is null || !bool.TryParse(row.Value, out var b) || b;

            lock (_cacheLock)
            {
                _cachedValue = value;
                _cacheExpiry = DateTimeOffset.UtcNow.AddSeconds(30);
            }
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read SignalR:Enabled from DB. Defaulting to true.");
            return true;
        }
    }

    public async Task SetEnabledAsync(bool enabled, CancellationToken ct = default)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var row = await db.AppSettings.IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Key == Key, ct);

            if (row is null)
            {
                row = new Core.Entities.AppSettings
                {
                    Id = Guid.NewGuid(),
                    Key = Key,
                    Value = enabled.ToString().ToLower() // Sparar som "true"/"false"
                };
                db.AppSettings.Add(row);
            }
            else
            {
                row.Value = enabled.ToString().ToLower();
                db.Entry(row).State = EntityState.Modified;
            }

            await db.SaveChangesAsync(ct);

            // Uppdatera cachen direkt
            lock (_cacheLock)
            {
                _cachedValue = enabled;
                _cacheExpiry = DateTimeOffset.UtcNow.AddSeconds(30);
            }
            _logger.LogInformation("SignalR hub {State} by admin.", enabled ? "enabled" : "disabled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save SignalR:Enabled to DB.");
            throw; // Kasta vidare så UI kan visa felmeddelande
        }
    }
}