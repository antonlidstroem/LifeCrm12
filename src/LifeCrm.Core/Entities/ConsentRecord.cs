// src/LifeCrm.Core/Entities/ConsentRecord.cs
using System.Net.Mime;
using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities;

/// <summary>
/// Immutable audit trail of consent decisions.
/// One row per grant or withdrawal event — never updated, only appended.
/// </summary>
public class ConsentRecord : BaseEntity
{
    // Who gave / withdrew consent
    public Guid ContactId { get; set; }
    public Contact? Contact { get; set; }

    // What type of processing they consented to (or withdrew)
    public ConsentType ConsentType { get; set; }

    // Granted = true, Withdrawn = false
    public bool IsGranted { get; set; }

    // Which version of the Privacy Policy / Terms was shown
    public string PolicyVersion { get; set; } = string.Empty;

    // Free-text: "web-form", "csv-import", "email-link", "admin-manual"
    public string Source { get; set; } = string.Empty;

    // The user who recorded this (null = system/automated)
    public Guid? RecordedByUserId { get; set; }

    // IP address of the data subject at time of consent (nullable for admin-entered)
    public string? IpAddress { get; set; }

    // UTC timestamp — CreatedAt from BaseEntity serves as the consent timestamp
    // LastModifiedAt / IsDeleted intentionally NOT used — records are immutable
}