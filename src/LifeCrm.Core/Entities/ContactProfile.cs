using LifeCrm.Core.Attributes;

namespace LifeCrm.Core.Entities;

/// <summary>
/// Admin-only enrichment data for a contact.
///
/// DESIGN DECISION — separate table (not columns on Contact):
///   1. Impossible to accidentally expose in standard Contact queries.
///   2. Hard-delete on erasure = one row, no UPDATE with 20 null fields.
///   3. Clear code-review boundary: any Include(c => c.Profile) is immediately visible.
///   4. Requires explicit Include — no lazy-loading risk.
///
/// MUST NOT be returned from any endpoint without [Authorize(Policy="AdminOnly")].
/// MUST NOT be created without DataEnrichmentConsented = true.
/// </summary>
[AdminOnly("Contains gifts, interests, spiritual notes — sensitive admin enrichment.")]
public class ContactProfile : TenantEntity
{
    // 1:1 with Contact (ContactId is a unique FK)
    public Guid     ContactId                { get; set; }
    public Contact? Contact                  { get; set; }

    // Free text throughout — enums would over-simplify and vary between organisations
    public string?  GiftsAndTalents          { get; set; }
    public string?  Interests                { get; set; }

    // What are they open to? e.g. "leading worship, hospitality, children's ministry"
    public string?  InvolvementOpenTo        { get; set; }

    // Deliberately free text — a person's spiritual journey must not become a checkbox
    // MUST NOT be used for automated scoring or filtering
    public string?  SpiritualNotes           { get; set; }

    // Visible to admin only — never in standard DSR export unless admin requests full export
    public string?  StaffPrivateNotes        { get; set; }

    // ── Consent ───────────────────────────────────────────────────────────
    // Profile MUST NOT be created without the contact's explicit consent
    public bool     DataEnrichmentConsented  { get; set; } = false;
    public DateTimeOffset? ConsentedAt       { get; set; }
    public string?  ConsentSource            { get; set; }  // "verbal-admin" | "written-form"

    public string   LastUpdatedBy            { get; set; } = "system";
}
