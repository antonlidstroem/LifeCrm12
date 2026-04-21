using System.Text.Json;
using LifeCrm.Core.Entities;
using LifeCrm.Infrastructure.Persistence;
using LifeCrm.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.BackgroundServices;

/// <summary>
/// Drains the AuditOutbox table into PropertyAuditLog every 5 seconds.
/// Runs independently of the request path — API latency is unaffected.
///
/// Idempotency: IsProcessed flag prevents double-processing on restart.
/// Fault tolerance: RetryCount prevents infinite loops on bad payloads (max 5 retries).
/// Batching: processes up to 200 rows per tick to limit transaction size.
/// </summary>
public class AuditOutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditOutboxProcessor> _logger;

    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize   = 200;
    private const int MaxRetries  = 5;

    public AuditOutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<AuditOutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AuditOutboxProcessor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "AuditOutboxProcessor encountered an error — will retry in {Interval}s.",
                    TickInterval.TotalSeconds);
            }

            await Task.Delay(TickInterval, stoppingToken);
        }

        _logger.LogInformation("AuditOutboxProcessor stopped.");
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Fetch unprocessed rows that haven't exceeded the retry limit
        var batch = await db.Set<AuditOutbox>()
            .Where(o => !o.IsProcessed && o.RetryCount < MaxRetries)
            .OrderBy(o => o.Id)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (batch.Count == 0) return;

        var auditRows = new List<PropertyAuditLog>(batch.Count * 4);

        foreach (var outbox in batch)
        {
            try
            {
                var entries = JsonSerializer
                    .Deserialize<List<PropertyAuditPayloadEntry>>(outbox.Payload);

                if (entries is not null)
                {
                    auditRows.AddRange(entries.Select(e => new PropertyAuditLog
                    {
                        OrganizationId  = e.OrganizationId,
                        EntityName      = e.EntityName,
                        EntityId        = e.EntityId,
                        PropertyName    = e.PropertyName,
                        Action          = e.Action,
                        OldValue        = e.OldValue,
                        NewValue        = e.NewValue,
                        ChangedByUserId = e.ChangedByUserId,
                        ChangedAt       = e.ChangedAt
                    }));
                }

                outbox.IsProcessed = true;
                outbox.ProcessedAt = DateTimeOffset.UtcNow;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex,
                    "Failed to deserialize AuditOutbox row {Id} — incrementing retry count ({Retry}/{Max}).",
                    outbox.Id, outbox.RetryCount + 1, MaxRetries);
                outbox.RetryCount++;
            }
        }

        if (auditRows.Count > 0)
            db.Set<PropertyAuditLog>().AddRange(auditRows);

        await db.SaveChangesAsync(ct);

        var processed = batch.Count(o => o.IsProcessed);
        if (processed > 0)
            _logger.LogDebug(
                "AuditOutboxProcessor: flushed {Flushed} entries from {Rows} outbox rows.",
                auditRows.Count, processed);
    }
}
