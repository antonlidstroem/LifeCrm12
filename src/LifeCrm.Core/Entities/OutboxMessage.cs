namespace LifeCrm.Core.Entities;

/// <summary>
/// Transactional outbox for async audit log processing.
/// Written in the same DB transaction as the business change.
/// Drained by AuditOutboxProcessor background service.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>"PropertyAuditLog" | "ConsentEvent"</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>JSON payload. For PropertyAuditLog: serialized PropertyAuditEntry.</summary>
    public string Payload { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }

    /// <summary>Incremented on each failed processing attempt. Max 5 before poison-queue.</summary>
    public int RetryCount { get; set; }

    public string? Error { get; set; }
}