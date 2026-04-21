namespace LifeCrm.Core.Entities;

/// <summary>
/// Transactional outbox for property-level audit entries.
/// Written in the SAME DB transaction as the business change (via PropertyAuditInterceptor).
/// Flushed to PropertyAuditLog by AuditOutboxProcessor (BackgroundService).
///
/// This pattern guarantees audit records are consistent with the data they describe:
/// if the business save fails, the outbox row is also rolled back.
/// </summary>
public class AuditOutbox
{
    public long           Id          { get; set; }

    /// <summary>JSON-serialized List&lt;PropertyAuditLogDto&gt;.</summary>
    public string         Payload     { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt   { get; set; }
    public bool           IsProcessed { get; set; } = false;
    public DateTimeOffset? ProcessedAt { get; set; }

    /// <summary>Incremented on deserialization errors. Rows with RetryCount &gt;= 5 are abandoned.</summary>
    public int            RetryCount  { get; set; } = 0;
}
