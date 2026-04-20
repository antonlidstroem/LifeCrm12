// src/LifeCrm.Core/Entities/PropertyAuditLog.cs
namespace LifeCrm.Core.Entities;

/// <summary>
/// Immutable, field-level audit record. Written via outbox — never inline.
/// NOT a TenantEntity because audit logs must survive org deletion.
/// </summary>
public class PropertyAuditLog
{
    public long Id { get; set; }   // bigint identity — high volume
    public Guid OrganizationId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;  // Created|Modified|Deleted
    public string? OldValue { get; set; }   // JSON-serialized, null on Create
    public string? NewValue { get; set; }   // JSON-serialized, null on Delete
    public Guid? ChangedByUserId { get; set; }
    public string? ChangedByEmail { get; set; }  // Denormalized — survives user deletion
    public DateTimeOffset ChangedAt { get; set; }
}