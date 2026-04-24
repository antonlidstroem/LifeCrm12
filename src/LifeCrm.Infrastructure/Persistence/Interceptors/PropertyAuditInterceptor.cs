using System.Text.Json;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that captures property-level changes and writes them to the
/// OutboxMessages table in the same transaction. A background service (AuditOutboxProcessor)
/// drains the outbox and writes to AuditLogs asynchronously.
///
/// Design decisions:
/// - Same-transaction outbox write: zero additional latency on the hot path.
/// - PII masking: hashed, not plain text, so audit log is safe if extracted.
/// - Skips housekeeping columns (LastModifiedAt, etc.) to reduce noise.
/// - Guards against stack overflow: OutboxMessage changes are not re-audited.
/// </summary>
public class PropertyAuditInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> PiiProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "Email", "Phone", "FirstName", "LastName", "Name",
        "AddressLine1", "AddressLine2", "City", "PostalCode",
        "IpAddressHash", "PasswordHash", "Notes"
    };

    private static readonly HashSet<string> AuditedEntityTypes = new(StringComparer.Ordinal)
    {
        nameof(Contact),
        nameof(Donation),
        nameof(ApplicationUser),
        nameof(ConsentRecord)
    };

    private static readonly HashSet<string> SkippedProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "LastModifiedAt", "LastModifiedBy", "CreatedAt", "DeletedAt", "EmailHash"
    };

    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<PropertyAuditInterceptor> _logger;

    public PropertyAuditInterceptor(
        ICurrentUserService currentUser,
        IHttpContextAccessor http,
        ILogger<PropertyAuditInterceptor> logger)
    {
        _currentUser = currentUser;
        _http = http;
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        if (eventData.Context is null) return new ValueTask<InterceptionResult<int>>(result);

        try
        {
            var outboxEntries = CollectOutboxEntries(eventData.Context);
            if (outboxEntries.Count > 0)
            {
                // Direct add to change tracker — same SaveChanges call, same transaction.
                eventData.Context.Set<OutboxMessage>().AddRange(outboxEntries);
            }
        }
        catch (Exception ex)
        {
            // CRITICAL: audit failure must NEVER block the business operation.
            _logger.LogError(ex, "PropertyAuditInterceptor failed to collect audit entries. Business operation will proceed.");
        }

        return new ValueTask<InterceptionResult<int>>(result);
    }

    private List<OutboxMessage> CollectOutboxEntries(DbContext ctx)
    {
        var outbox = new List<OutboxMessage>();
        var correlationId = _http.HttpContext?.TraceIdentifier;
        var userId = _currentUser.UserId?.ToString() ?? "system";
        var orgId = _currentUser.OrganizationId ?? Guid.Empty;
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ctx.ChangeTracker.Entries())
        {
            var typeName = entry.Entity.GetType().Name;

            // Skip entity types we don't audit, and skip OutboxMessage itself (prevent recursion)
            if (!AuditedEntityTypes.Contains(typeName)) continue;
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;

            var entityId = entry.Properties
                .FirstOrDefault(p => p.Metadata.Name == "Id")?.CurrentValue as Guid? ?? Guid.Empty;

            if (entry.State is EntityState.Added or EntityState.Deleted)
            {
                // Row-level event — no property detail
                outbox.Add(Wrap(new PropertyAuditEntry
                {
                    OrganizationId = orgId,
                    EntityName = typeName,
                    EntityId = entityId,
                    Action = entry.State.ToString(),
                    ChangedBy = userId,
                    ChangedAt = now,
                    CorrelationId = correlationId
                }));
                continue;
            }

            // Modified — emit one entry per changed property
            foreach (var prop in entry.Properties)
            {
                if (!prop.IsModified) continue;
                if (SkippedProperties.Contains(prop.Metadata.Name)) continue;

                outbox.Add(Wrap(new PropertyAuditEntry
                {
                    OrganizationId = orgId,
                    EntityName = typeName,
                    EntityId = entityId,
                    Action = "Modified",
                    PropertyName = prop.Metadata.Name,
                    OldValue = SafeSerialize(prop.Metadata.Name, prop.OriginalValue),
                    NewValue = SafeSerialize(prop.Metadata.Name, prop.CurrentValue),
                    ChangedBy = userId,
                    ChangedAt = now,
                    CorrelationId = correlationId
                }));
            }
        }

        return outbox;
    }

    private static string SafeSerialize(string propertyName, object? value)
    {
        if (value is null) return "[null]";
        var str = value.ToString() ?? "[null]";

        if (PiiProperties.Contains(propertyName))
        {
            // Store one-way hash — confirms WHAT changed without storing WHAT the value was.
            var hashBytes = System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(str));
            return $"[PII-HASH:{Convert.ToHexString(hashBytes)[..16]}]";
        }

        // Truncate very long values (HtmlBody etc.) to prevent huge audit rows
        return str.Length > 500 ? str[..500] + "[TRUNCATED]" : str;
    }

    private static OutboxMessage Wrap(PropertyAuditEntry entry) => new()
    {
        Type = "PropertyAuditLog",
        Payload = JsonSerializer.Serialize(entry, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        })
    };
}

/// <summary>Internal DTO serialized into OutboxMessage.Payload.</summary>
public record PropertyAuditEntry
{
    public Guid OrganizationId { get; init; }
    public string EntityName { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string? PropertyName { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public string ChangedBy { get; init; } = "system";
    public DateTimeOffset ChangedAt { get; init; }
    public string? CorrelationId { get; init; }
}