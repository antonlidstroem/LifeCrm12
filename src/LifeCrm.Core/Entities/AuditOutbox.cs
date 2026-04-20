// src/LifeCrm.Core/Entities/AuditOutbox.cs
namespace LifeCrm.Core.Entities;

/// <summary>
/// Transactional outbox for property-level audit entries.
/// Written in the same DB transaction as the business change.
/// Flushed to PropertyAuditLog by AuditOutboxProcessor (BackgroundService).
/// </summary>
public class AuditOutbox
{
    public long Id { get; set; }
    public string Payload { get; set; } = string.Empty; // JSON: PropertyAuditLog[]
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsProcessed { get; set; } = false;
    public DateTimeOffset? ProcessedAt { get; set; }
    public int RetryCount { get; set; } = 0;
}