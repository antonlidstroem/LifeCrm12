using System.Text.Json;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LifeCrm.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Captures property-level changes on audited entities and writes a single
/// AuditOutbox row in the SAME DB transaction as the business save.
///
/// The outbox row is then drained asynchronously by AuditOutboxProcessor
/// — zero latency added to the hot request path.
///
/// Only entities listed in AuditedEntities receive field-level tracking.
/// Properties listed in ExcludedProperties are always skipped (high-frequency
/// auto-set fields that would generate noise without compliance value).
/// </summary>
public class PropertyAuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    private static readonly HashSet<string> ExcludedProperties = new(StringComparer.Ordinal)
    {
        nameof(BaseEntity.LastModifiedAt),
        nameof(BaseEntity.LastModifiedBy),
        nameof(BaseEntity.CreatedAt),
        nameof(BaseEntity.DeletedAt),
        nameof(BaseEntity.IsDeleted)
    };

    // Only entities that hold PII or consent data receive field-level tracking.
    // Adding an entity here is the only change required to extend coverage.
    private static readonly HashSet<string> AuditedEntities = new(StringComparer.Ordinal)
    {
        nameof(Contact),
        nameof(ApplicationUser),
        nameof(ConsentRecord)
    };

    public PropertyAuditInterceptor(ICurrentUserService currentUser)
        => _currentUser = currentUser;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        if (eventData.Context is null)
            return new ValueTask<InterceptionResult<int>>(result);

        var entries = CollectAuditEntries(eventData.Context);
        if (entries.Count == 0)
            return new ValueTask<InterceptionResult<int>>(result);

        var outboxRow = new AuditOutbox
        {
            Payload   = JsonSerializer.Serialize(entries),
            CreatedAt = DateTimeOffset.UtcNow
        };

        eventData.Context.Set<AuditOutbox>().Add(outboxRow);

        return new ValueTask<InterceptionResult<int>>(result);
    }

    private List<PropertyAuditPayloadEntry> CollectAuditEntries(DbContext context)
    {
        var result = new List<PropertyAuditPayloadEntry>();
        var now    = DateTimeOffset.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            var entityName = entry.Entity.GetType().Name;
            if (!AuditedEntities.Contains(entityName)) continue;

            var action = entry.State switch
            {
                EntityState.Added    => "Created",
                EntityState.Modified => "Modified",
                EntityState.Deleted  => "Deleted",
                _                   => null
            };
            if (action is null) continue;

            var orgId = entry.Entity is TenantEntity te ? te.OrganizationId : Guid.Empty;

            foreach (var prop in entry.Properties)
            {
                if (ExcludedProperties.Contains(prop.Metadata.Name)) continue;

                // For Modified state, skip properties that did not actually change
                if (entry.State == EntityState.Modified && !prop.IsModified) continue;

                var oldVal = entry.State == EntityState.Added
                    ? null
                    : SerializeValue(prop.OriginalValue);

                var newVal = entry.State == EntityState.Deleted
                    ? null
                    : SerializeValue(prop.CurrentValue);

                // Skip no-op changes (value set to itself)
                if (oldVal == newVal) continue;

                result.Add(new PropertyAuditPayloadEntry
                {
                    OrganizationId  = orgId,
                    EntityName      = entityName,
                    EntityId        = entry.Entity.Id,
                    PropertyName    = prop.Metadata.Name,
                    Action          = action,
                    OldValue        = oldVal,
                    NewValue        = newVal,
                    ChangedByUserId = userId,
                    ChangedAt       = now
                });
            }
        }

        return result;
    }

    private static string? SerializeValue(object? value)
    {
        if (value is null) return null;

        // Avoid double-encoding strings — store them as their raw value
        if (value is string s) return s;

        return JsonSerializer.Serialize(value);
    }
}

/// <summary>
/// Lightweight payload DTO written into AuditOutbox.Payload (JSON).
/// Kept internal to Infrastructure — the outbox processor reads this same type.
/// </summary>
internal record PropertyAuditPayloadEntry
{
    public Guid    OrganizationId   { get; init; }
    public string  EntityName       { get; init; } = string.Empty;
    public Guid    EntityId         { get; init; }
    public string  PropertyName     { get; init; } = string.Empty;
    public string  Action           { get; init; } = string.Empty;
    public string? OldValue         { get; init; }
    public string? NewValue         { get; init; }
    public Guid?   ChangedByUserId  { get; init; }
    public DateTimeOffset ChangedAt { get; init; }
}
