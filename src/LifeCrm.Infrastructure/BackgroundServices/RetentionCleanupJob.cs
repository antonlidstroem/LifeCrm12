using LifeCrm.Core.Entities;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.BackgroundServices;

/// <summary>
/// Enforces data retention policies configured in the AppSettings DB table.
///
/// Runs once per day. Each policy is stored as a key/value pair:
///   Key:   "Retention:{EntityName}"
///   Value: "{Days}" (integer) or "0" to disable cleanup for that entity.
///
/// Defaults (applied when no DB setting is found):
///   PropertyAuditLog: 2555 days (7 years — GDPR accountability requirement)
///   AuditOutbox:       30 days (processed rows only)
///   DsrExportJob:      90 days
///
/// DryRun mode: set AppSettings "Retention:DryRun" = "true" to log what
/// would be deleted without actually deleting. Safe to enable in production
/// for auditing the cleanup scope before committing.
/// </summary>
public class RetentionCleanupJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RetentionCleanupJob> _logger;

    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24);

    // Defaults in days. Zero = disabled.
    private static readonly Dictionary<string, int> DefaultRetentionDays = new()
    {
        ["PropertyAuditLog"] = 2555,   // 7 years
        ["AuditOutbox"]      = 30,
        ["DsrExportJob"]     = 90,
    };

    public RetentionCleanupJob(
        IServiceScopeFactory scopeFactory,
        ILogger<RetentionCleanupJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RetentionCleanupJob started.");

        // Stagger startup by 5 minutes so the job doesn't compete with
        // the initial application boot and database migration.
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RetentionCleanupJob error — will retry in {Interval}h.",
                    RunInterval.TotalHours);
            }

            await Task.Delay(RunInterval, stoppingToken);
        }

        _logger.LogInformation("RetentionCleanupJob stopped.");
    }

    private async Task RunCleanupAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var settings = await db.AppSettings
            .IgnoreQueryFilters()
            .Where(s => s.Key.StartsWith("Retention:"))
            .ToDictionaryAsync(s => s.Key, s => s.Value, ct);

        var dryRun = settings.TryGetValue("Retention:DryRun", out var dryRunStr)
            && bool.TryParse(dryRunStr, out var dryRunVal) && dryRunVal;

        if (dryRun)
            _logger.LogInformation("RetentionCleanupJob running in DryRun mode — no data will be deleted.");

        await CleanPropertyAuditLogsAsync(db, settings, dryRun, ct);
        await CleanAuditOutboxAsync(db, settings, dryRun, ct);
        await CleanDsrExportJobsAsync(db, settings, dryRun, ct);
    }

    private async Task CleanPropertyAuditLogsAsync(
        AppDbContext db, Dictionary<string, string> settings, bool dryRun, CancellationToken ct)
    {
        var days = GetRetentionDays(settings, "PropertyAuditLog");
        if (days == 0) return;

        var cutoff = DateTimeOffset.UtcNow.AddDays(-days);
        var count  = await db.Set<PropertyAuditLog>()
            .CountAsync(l => l.ChangedAt < cutoff, ct);

        _logger.LogInformation(
            "RetentionCleanupJob: {Count} PropertyAuditLog rows older than {Days} days.",
            count, days);

        if (!dryRun && count > 0)
        {
            // Use ExecuteDeleteAsync for bulk efficiency — avoids loading rows into memory
            await db.Set<PropertyAuditLog>()
                .Where(l => l.ChangedAt < cutoff)
                .ExecuteDeleteAsync(ct);
        }
    }

    private async Task CleanAuditOutboxAsync(
        AppDbContext db, Dictionary<string, string> settings, bool dryRun, CancellationToken ct)
    {
        var days = GetRetentionDays(settings, "AuditOutbox");
        if (days == 0) return;

        var cutoff = DateTimeOffset.UtcNow.AddDays(-days);
        var count  = await db.Set<AuditOutbox>()
            .CountAsync(o => o.IsProcessed && o.CreatedAt < cutoff, ct);

        _logger.LogInformation(
            "RetentionCleanupJob: {Count} processed AuditOutbox rows older than {Days} days.",
            count, days);

        if (!dryRun && count > 0)
        {
            await db.Set<AuditOutbox>()
                .Where(o => o.IsProcessed && o.CreatedAt < cutoff)
                .ExecuteDeleteAsync(ct);
        }
    }

    private async Task CleanDsrExportJobsAsync(
        AppDbContext db, Dictionary<string, string> settings, bool dryRun, CancellationToken ct)
    {
        var days = GetRetentionDays(settings, "DsrExportJob");
        if (days == 0) return;

        var cutoff = DateTimeOffset.UtcNow.AddDays(-days);
        var count  = await db.Set<DsrExportJob>()
            .CountAsync(j => j.CreatedAt < cutoff, ct);

        _logger.LogInformation(
            "RetentionCleanupJob: {Count} DsrExportJob rows older than {Days} days.",
            count, days);

        if (!dryRun && count > 0)
        {
            // Null out the export payload first to release LOB storage,
            // then delete the job metadata rows.
            await db.Set<DsrExportJob>()
                .Where(j => j.CreatedAt < cutoff && j.ExportPayload != null)
                .ExecuteUpdateAsync(s => s.SetProperty(j => j.ExportPayload, (byte[]?)null), ct);

            await db.Set<DsrExportJob>()
                .Where(j => j.CreatedAt < cutoff)
                .ExecuteDeleteAsync(ct);
        }
    }

    private static int GetRetentionDays(Dictionary<string, string> settings, string entityName)
    {
        var key = $"Retention:{entityName}";
        if (settings.TryGetValue(key, out var val) && int.TryParse(val, out var days))
            return days;
        return DefaultRetentionDays.GetValueOrDefault(entityName, 0);
    }
}
