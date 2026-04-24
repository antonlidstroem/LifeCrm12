using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.BackgroundServices;

/// <summary>
/// Nightly background service that enforces data retention policies.
/// Processes one organisation at a time to bound memory and lock contention.
/// Uses a DB advisory lock (AppSettings row) so only one instance runs in multi-pod deployments.
/// </summary>
public class RetentionWorker : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24);
    private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(5); // let app warm up first

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RetentionWorker> _logger;

    public RetentionWorker(IServiceScopeFactory sf, ILogger<RetentionWorker> logger)
    {
        _scopeFactory = sf;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RetentionWorker started. Initial delay {Delay}.", InitialDelay);
        await Task.Delay(InitialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunRetentionPassAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RetentionWorker pass failed.");
            }

            await Task.Delay(RunInterval, stoppingToken);
        }
    }

    private async Task RunRetentionPassAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dsrService = scope.ServiceProvider.GetRequiredService<IDsrService>();

        // Acquire advisory lock via AppSettings table
        if (!await TryAcquireLockAsync(db, ct)) return;

        try
        {
            var policies = await db.Set<RetentionPolicy>()
                .IgnoreQueryFilters()
                .Where(p => p.IsActive && !p.IsDeleted)
                .ToListAsync(ct);

            foreach (var policy in policies)
            {
                await ProcessPolicyAsync(db, dsrService, policy, ct);
            }
        }
        finally
        {
            await ReleaseLockAsync(db, ct);
        }
    }

    private async Task ProcessPolicyAsync(
        AppDbContext db, IDsrService dsrService, RetentionPolicy policy, CancellationToken ct)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-policy.RetentionDays);
        int affected = 0;

        _logger.LogInformation(
            "Retention: Processing policy {EntityName} for org {OrgId}, cutoff {Cutoff}, action {Action}.",
            policy.EntityName, policy.OrganizationId, cutoff, policy.Action);

        if (policy.EntityName == nameof(Contact))
        {
            // Find contacts with no activity since cutoff and no recent donations
            var staleContacts = await db.Contacts
                .IgnoreQueryFilters()
                .Where(c => c.OrganizationId == policy.OrganizationId
                         && !c.IsDeleted
                         && c.CreatedAt < cutoff
                         && !c.Donations.Any(d => d.Date >= DateOnly.FromDateTime(cutoff.DateTime))
                         && !c.Interactions.Any(i => i.OccurredAt >= cutoff))
                .Select(c => c.Id)
                .ToListAsync(ct);

            foreach (var id in staleContacts)
            {
                try
                {
                    switch (policy.Action)
                    {
                        case RetentionAction.Anonymize:
                            await dsrService.AnonymizeAsync(id, "RetentionPolicy", true, ct);
                            break;
                        case RetentionAction.HardDelete:
                            await dsrService.HardDeleteAsync(id, ct);
                            break;
                        case RetentionAction.Archive:
                            // Future: move to archive table
                            _logger.LogWarning("Archive action not yet implemented for Contact {Id}.", id);
                            break;
                    }
                    affected++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RetentionWorker failed for Contact {Id}.", id);
                }
            }
        }
        else if (policy.EntityName == "AuditLog")
        {
            // Hard-delete old audit logs — they are operational, not business data
            affected = await db.AuditLogs
                .Where(a => a.ChangedAt < cutoff)
                .ExecuteDeleteAsync(ct);
        }
        else if (policy.EntityName == "OutboxMessage")
        {
            // Purge processed outbox messages older than cutoff
            affected = await db.Set<OutboxMessage>()
                .Where(m => m.ProcessedAt != null && m.CreatedAt < cutoff)
                .ExecuteDeleteAsync(ct);
        }

        policy.LastRunAt = DateTimeOffset.UtcNow;
        policy.LastRunAffected = affected;
        db.Entry(policy).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Retention: {EntityName} org {OrgId} — {Count} records processed.",
            policy.EntityName, policy.OrganizationId, affected);
    }

    private const string LockKey = "RetentionWorker:Lock";

    private async Task<bool> TryAcquireLockAsync(AppDbContext db, CancellationToken ct)
    {
        var existing = await db.AppSettings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Key == LockKey, ct);

        if (existing is not null)
        {
            // Check if lock is stale (held > 2 hours = crashed instance)
            if (DateTimeOffset.TryParse(existing.Value, out var lockTime)
                && DateTimeOffset.UtcNow - lockTime < TimeSpan.FromHours(2))
            {
                _logger.LogInformation("RetentionWorker: Lock held by another instance. Skipping.");
                return false;
            }
            existing.Value = DateTimeOffset.UtcNow.ToString("O");
            db.Entry(existing).State = EntityState.Modified;
        }
        else
        {
            db.AppSettings.Add(new AppSettings
            {
                Id = Guid.NewGuid(),
                Key = LockKey,
                Value = DateTimeOffset.UtcNow.ToString("O")
            });
        }

        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another instance beat us to it
            return false;
        }
    }

    private async Task ReleaseLockAsync(AppDbContext db, CancellationToken ct)
    {
        var row = await db.AppSettings.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Key == LockKey, ct);
        if (row is not null)
        {
            db.AppSettings.Remove(row);
            await db.SaveChangesAsync(ct);
        }
    }
}