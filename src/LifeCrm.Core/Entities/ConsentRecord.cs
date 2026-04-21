using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities;

/// <summary>
/// Immutable audit trail of consent decisions.
/// One row per grant or withdrawal event — never updated, only appended.
/// Do not inherit TenantEntity: OrganizationId is denormalized here so consent
/// records survive org restructuring.
/// </summary>
public class ConsentRecord : BaseEntity
{
    public Guid   ContactId          { get; set; }
    public Contact? Contact          { get; set; }

    public Guid   OrganizationId     { get; set; }

    /// <summary>What type of processing the data subject is consenting to (or withdrawing).</summary>
    public ConsentType ConsentType   { get; set; }

    /// <summary>true = granted, false = withdrawn.</summary>
    public bool   IsGranted          { get; set; }

    /// <summary>Which version of the Privacy Policy / Terms was presented at time of decision.</summary>
    public string PolicyVersion      { get; set; } = string.Empty;

    /// <summary>
    /// Free-text source descriptor.
    /// Examples: "web-form", "csv-import", "email-link", "admin-manual", "unsubscribe-link".
    /// </summary>
    public string Source             { get; set; } = string.Empty;

    /// <summary>The staff user who recorded this entry. Null = automated / self-service.</summary>
    public Guid?  RecordedByUserId   { get; set; }

    /// <summary>IP address of the data subject at time of consent. Null for admin-entered records.</summary>
    public string? IpAddress         { get; set; }

    // CreatedAt (inherited from BaseEntity) = the canonical consent timestamp.
    // LastModifiedAt / IsDeleted are inherited but MUST NOT be used — records are immutable.
}
