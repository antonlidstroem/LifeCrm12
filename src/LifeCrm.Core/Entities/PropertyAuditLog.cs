namespace LifeCrm.Core.Entities;

/// <summary>
/// Immutable, field-level audit record.
/// Written asynchronously by AuditOutboxProcessor — never inline in the request path.
/// Uses bigint identity (not Guid) because this table is high-volume.
/// NOT a TenantEntity: audit logs must survive org deletion for regulatory purposes.
/// </summary>
public class PropertyAuditLog
{
    public long   Id               { get; set; }
    public Guid   OrganizationId   { get; set; }
    public string EntityName       { get; set; } = string.Empty;
    public Guid   EntityId         { get; set; }
    public string PropertyName     { get; set; } = string.Empty;

    /// <summary>Created | Modified | Deleted</summary>
    public string Action           { get; set; } = string.Empty;

    /// <summary>JSON-serialized previous value. Null on Created.</summary>
    public string? OldValue        { get; set; }

    /// <summary>JSON-serialized new value. Null on Deleted.</summary>
    public string? NewValue        { get; set; }

    public Guid?  ChangedByUserId  { get; set; }

    /// <summary>
    /// Denormalized email of the acting user. Survives user deletion.
    /// Set by AuditOutboxProcessor from the stored payload.
    /// </summary>
    public string? ChangedByEmail  { get; set; }

    public DateTimeOffset ChangedAt { get; set; }
}
