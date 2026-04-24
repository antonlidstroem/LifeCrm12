using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities;

/// <summary>
/// Immutable, append-only record of a consent event.
/// Never UPDATE rows — always INSERT new ones.
/// The latest row per (ContactId, ConsentType) is the authoritative state.
/// </summary>
public class ConsentRecord : TenantEntity
{
    public Guid ContactId { get; set; }
    public ConsentType ConsentType { get; set; }
    public ConsentStatus Status { get; set; }

    /// <summary>
    /// Semantic version of the privacy policy at time of consent.
    /// e.g. "2024-05-01", "v3.2". Allows re-soliciting consent when policy changes.
    /// </summary>
    public string PolicyVersion { get; set; } = string.Empty;

    /// <summary>
    /// Legal basis under GDPR Art. 6.
    /// e.g. "Consent", "LegitimateInterest", "Contract", "LegalObligation"
    /// </summary>
    public string LegalBasis { get; set; } = "Consent";

    /// <summary>How consent was collected. e.g. "web-form", "import", "manual", "dsr-api"</summary>
    public string? Channel { get; set; }

    /// <summary>Hashed IP address of the data subject at time of consent (never plain text).</summary>
    public string? IpAddressHash { get; set; }

    /// <summary>UserId of the operator who recorded this, or "system".</summary>
    public string RecordedBy { get; set; } = "system";

    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;

    public string? Notes { get; set; }

    // Navigation
    public Contact? Contact { get; set; }
}