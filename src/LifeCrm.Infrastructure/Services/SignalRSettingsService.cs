using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.Services;

/// <summary>
/// Persists the SignalR enabled/disabled toggle in the AppSettings table.
/// Uses a volatile bool cache (30-second TTL) so every hub message does not
/// require a database round-trip. Thread-safe via Interlocked.
/// </summary>
public class SignalRSettingsService : ISignalRSettings
{
    private const string Key = "SignalR:Enabled";

    // Static cache — shared across all scoped instances in the process
    private static volatile bool   _cachedValue  = true;
    private static DateTimeOffset  _cacheExpiry  = DateTimeOffset.MinValue;
    private static readonly object _cacheLock    = new();

    private readonly AppDbContext _db;
    private readonly ILogger<SignalRSettingsService> _logger;

    public SignalRSettingsService(AppDbContext db, ILogger<SignalRSettingsService> logger)
    { _db = db; _logger = logger; }

    public async Task<bool> GetEnabledAsync(CancellationToken ct = default)
    {
        if (DateTimeOffset.UtcNow < _cacheExpiry) return _cachedValue;

        try
        {
            var row = await _db.AppSettings.IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Key == Key, ct);
            var value = row is null || bool.TryParse(row.Value, out var b) && b;
            lock (_cacheLock) { _cachedValue = value; _cacheExpiry = DateTimeOffset.UtcNow.AddSeconds(30); }
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
        var row = await _db.AppSettings.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Key == Key, ct);

        if (row is null)
        {
            row = new Core.Entities.AppSettings { Id = Guid.NewGuid(), Key = Key, Value = enabled.ToString() };
            _db.AppSettings.Add(row);
        }
        else
        {
            row.Value = enabled.ToString();
            _db.Entry(row).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }

        await _db.SaveChangesAsync(ct);

        // Invalidate cache immediately
        lock (_cacheLock) { _cachedValue = enabled; _cacheExpiry = DateTimeOffset.UtcNow.AddSeconds(30); }
        _logger.LogInformation("SignalR hub {State} by admin.", enabled ? "enabled" : "disabled");
    }
}
