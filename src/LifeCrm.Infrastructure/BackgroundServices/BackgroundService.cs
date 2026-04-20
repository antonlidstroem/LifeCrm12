// src/LifeCrm.Infrastructure/BackgroundServices/AuditOutboxProcessor.cs
using System.Text.Json;
using LifeCrm.Core.Entities;
using LifeCrm.Infrastructure.Persistence;
using LifeCrm.Infrastructure.Persistence.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.BackgroundServices;

/// <summary>
/// Drains the AuditOutbox table into PropertyAuditLog every 5 seconds.
/// Runs on a single instance — idempotent by IsProcessed flag.
/// </summary>
public class AuditOutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditOutboxProcessor> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 200;

    public AuditOutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<AuditOutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessBatchAsync(stoppingToken); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuditOutboxProcessor failed — will retry in {Interval}s.",
                    Interval.TotalSeconds);
            }
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var batch = await db.Set<AuditOutbox>()
            .Where(o => !o.IsProcessed && o.RetryCount < 5)
            .OrderBy(o => o.Id)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (batch.Count == 0) return;

        var auditRows = new List<PropertyAuditLog>();

        foreach (var outbox in batch)
        {
            try
            {
                var entries = JsonSerializer
                    .Deserialize<List<PropertyAuditLogDto>>(outbox.Payload);

                if (entries is not null)
                {
                    auditRows.AddRange(entries.Select(e => new PropertyAuditLog
                    {
                        OrganizationId = e.OrganizationId,
                        EntityName = e.EntityName,
                        EntityId = e.EntityId,
                        PropertyName = e.PropertyName,
                        Action = e.Action,
                        OldValue = e.OldValue,
                        NewValue = e.NewValue,
                        ChangedByUserId = e.ChangedByUserId,
                        ChangedAt = e.ChangedAt
                    }));
                }

                outbox.IsProcessed = true;
                outbox.ProcessedAt = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to deserialize AuditOutbox {Id}. Incrementing retry count.", outbox.Id);
                outbox.RetryCount++;
            }
        }

        db.Set<PropertyAuditLog>().AddRange(auditRows);
        await db.SaveChangesAsync(ct);

        _logger.LogDebug("AuditOutboxProcessor: flushed {Count} entries.", auditRows.Count);
    }
}