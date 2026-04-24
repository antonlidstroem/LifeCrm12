namespace LifeCrm.Core.Entities;

/// <summary>
/// Persisted audit record. Written asynchronously via OutboxMessage → AuditOutboxProcessor.
/// Property-level tracking added in Phase 1 — all new columns are nullable for BC.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = "system";
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

    // ── Phase 1 additions (nullable — backward compatible) ────────────────────

    /// <summary>
    /// null = row-level event (Created, Deleted).
    /// Populated = specific property changed.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// For PII fields: SHA-256 hash prefixed with "[PII-HASH:]".
    /// For non-PII: string representation of previous value.
    /// null for Created events.
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// For PII fields: SHA-256 hash prefixed with "[PII-HASH:]".
    /// For non-PII: string representation of new value.
    /// null for Deleted events.
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>HTTP request TraceIdentifier for correlating multiple changes in one request.</summary>
    public string? CorrelationId { get; set; }

    // Kept for backward compat with existing AuditBehaviour usage
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}