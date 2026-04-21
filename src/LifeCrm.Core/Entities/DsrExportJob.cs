namespace LifeCrm.Core.Entities;

/// <summary>
/// Tracks an async Data Subject Request export job.
/// Created by GdprController, processed by DsrExportService background work.
/// The resulting JSON payload is stored here until download or expiry.
/// </summary>
public class DsrExportJob : TenantEntity
{
    public Guid   ContactId       { get; set; }
    public string Status          { get; set; } = DsrExportStatus.Pending;

    /// <summary>Compressed JSON export — populated once Status = Completed.</summary>
    public byte[]? ExportPayload  { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Download link expires 24 hours after completion.</summary>
    public DateTimeOffset? ExpiresAt   { get; set; }

    public string? ErrorMessage   { get; set; }
    public Guid    RequestedByUserId { get; set; }
}

public static class DsrExportStatus
{
    public const string Pending   = "Pending";
    public const string Running   = "Running";
    public const string Completed = "Completed";
    public const string Failed    = "Failed";
    public const string Expired   = "Expired";
}
