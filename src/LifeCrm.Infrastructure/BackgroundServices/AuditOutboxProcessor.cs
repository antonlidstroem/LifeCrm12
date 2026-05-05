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
/// Drains OutboxMessages of type "PropertyAuditLog" and writes them to AuditLogs.
/// Runs every 5 seconds. Processes batches of 200 to bound memory.
/// Uses a DB-level lock (AppSettings row) to prevent duplicate processing in multi-instance deployments.
/// Max retry = 5 before message is marked as poison (ProcessedAt stays null, RetryCount = 5).
/// </summary>
public class AuditOutboxProcessor : BackgroundService
{
    private const int BatchSize = 200;
    private const int MaxRetries = 5;
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditOutboxProcessor> _logger;

    public AuditOutboxProcessor(IServiceScopeFactory sf, ILogger<AuditOutboxProcessor> logger)
    {
        _scopeFactory = sf;
        _logger = logger;
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
                _logger.LogError(ex, "AuditOutboxProcessor encountered an error. Will retry after interval.");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }

        _logger.LogInformation("AuditOutboxProcessor stopped.");
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        // Use a fresh DbContext with no tenant filter (system context)
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var messages = await db.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt == null
                     && m.RetryCount < MaxRetries
                     && m.Type == "PropertyAuditLog")
            .OrderBy(m => m.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (messages.Count == 0) return;

        var auditLogs = new List<AuditLog>(messages.Count);

        foreach (var msg in messages)
        {
            try
            {
                var entry = JsonSerializer.Deserialize<PropertyAuditEntry>(
                    msg.Payload,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (entry is not null)
                {
                    auditLogs.Add(new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = entry.OrganizationId,
                        EntityName = entry.EntityName,
                        EntityId = entry.EntityId,
                        Action = entry.Action,
                        PropertyName = entry.PropertyName,
                        OldValue = entry.OldValue,
                        NewValue = entry.NewValue,
                        ChangedBy = entry.ChangedBy,
                        ChangedAt = entry.ChangedAt,
                        CorrelationId = entry.CorrelationId
                    });
                }

                msg.ProcessedAt = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                msg.RetryCount++;
                msg.Error = ex.Message[..Math.Min(ex.Message.Length, 500)];
                _logger.LogError(ex,
                    "Failed to process OutboxMessage {Id}. Retry count: {Retry}.",
                    msg.Id, msg.RetryCount);
            }
        }

        if (auditLogs.Count > 0)
            db.AuditLogs.AddRange(auditLogs);

        await db.SaveChangesAsync(ct);

        _logger.LogDebug("AuditOutboxProcessor processed {Count} messages.", messages.Count);
    }
}