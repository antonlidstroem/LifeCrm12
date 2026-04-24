using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities;

/// <summary>
/// Per-organisation, per-entity-type data retention configuration.
/// Enforced nightly by RetentionWorker background service.
/// </summary>
public class RetentionPolicy : TenantEntity
{
    /// <summary>Entity type name. e.g. "Contact", "Donation", "Interaction", "AuditLog"</summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>Age in days after which the action is triggered. Default 2555 = ~7 years.</summary>
    public int RetentionDays { get; set; } = 2555;

    public RetentionAction Action { get; set; } = RetentionAction.Anonymize;

    /// <summary>
    /// When false, worker skips this policy. Allows orgs to pause without deleting the config.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Last time this policy was executed by the worker.</summary>
    public DateTimeOffset? LastRunAt { get; set; }

    /// <summary>Count of entities affected in last run.</summary>
    public int LastRunAffected { get; set; }
}