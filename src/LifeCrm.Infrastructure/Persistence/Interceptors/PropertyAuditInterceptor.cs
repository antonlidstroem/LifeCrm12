// src/LifeCrm.Infrastructure/Persistence/Interceptors/PropertyAuditInterceptor.cs
using System.Text.Json;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LifeCrm.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Captures property-level changes and writes an AuditOutbox row
/// in the SAME transaction as the business save. Zero hot-path latency.
/// </summary>
public class PropertyAuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    // Properties explicitly excluded from audit (e.g. bulk-updated timestamps)
    private static readonly HashSet<string> ExcludedProperties = new()
    {
        nameof(BaseEntity.LastModifiedAt),
        nameof(BaseEntity.LastModifiedBy),
        nameof(BaseEntity.CreatedAt),
        nameof(BaseEntity.DeletedAt)
    };

    // Entities that contain PII — only these get field-level tracking
    private static readonly HashSet<string> AuditedEntities = new()
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
            Payload = JsonSerializer.Serialize(entries),
            CreatedAt = DateTimeOffset.UtcNow
        };
        eventData.Context.Set<AuditOutbox>().Add(outboxRow);

        return new ValueTask<InterceptionResult<int>>(result);
    }

    private List<PropertyAuditLogDto> CollectAuditEntries(DbContext context)
    {
        var entries = new List<PropertyAuditLogDto>();
        var now = DateTimeOffset.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            var entityName = entry.Entity.GetType().Name;
            if (!AuditedEntities.Contains(entityName)) continue;

            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Modified",
                EntityState.Deleted => "Deleted",
                _ => null
            };
            if (action is null) continue;

            var orgId = entry.Entity is TenantEntity te ? te.OrganizationId : Guid.Empty;

            foreach (var prop in entry.Properties)
            {
                if (ExcludedProperties.Contains(prop.Metadata.Name)) continue;
                if (entry.State == EntityState.Modified && !prop.IsModified) continue;

                var oldVal = entry.State == EntityState.Added
                    ? null
                    : Serialize(prop.OriginalValue);

                var newVal = entry.State == EntityState.Deleted
                    ? null
                    : Serialize(prop.CurrentValue);

                if (oldVal == newVal) continue;

                entries.Add(new PropertyAuditLogDto
                {
                    OrganizationId = orgId,
                    EntityName = entityName,
                    EntityId = entry.Entity.Id,
                    PropertyName = prop.Metadata.Name,
                    Action = action,
                    OldValue = oldVal,
                    NewValue = newVal,
                    ChangedByUserId = userId,
                    ChangedAt = now
                });
            }
        }
        return entries;
    }

    private static string? Serialize(object? value)
        => value is null ? null : JsonSerializer.Serialize(value);
}

// Lightweight DTO for the outbox payload — avoids entity references
internal record PropertyAuditLogDto
{
    public Guid OrganizationId { get; init; }
    public string EntityName { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public string PropertyName { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public Guid? ChangedByUserId { get; init; }
    public DateTimeOffset ChangedAt { get; init; }
}