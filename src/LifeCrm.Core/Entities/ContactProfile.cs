// src/LifeCrm.Core/Entities/ContactProfile.cs
//
// DESIGN DECISION: Separate table, not columns on Contact.
// Reasons:
//   1. Makes it impossible to accidentally expose enrichment data in Contact queries
//   2. Clearly signals the data class boundary in code review
//   3. Can be excluded from DSR exports or handled separately per consent
//   4. Deletion is a single row delete — no UPDATE with 20 null fields
//
using LifeCrm.Core.Entities;

[AdminOnly]  // Custom marker attribute — enforced by API authorization
public class ContactProfile : TenantEntity
{
    // 1:1 with Contact — ContactId is also the PK (no separate Id needed)
    public Guid ContactId { get; set; }
    public Contact? Contact { get; set; }

    // ── Gifts and talents (church context) ───────────────────────────────
    // Free text — not a structured enum. Avoids over-engineering for
    // something that varies enormously between organizations.
    public string? GiftsAndTalents { get; set; }

    // ── Interests and passions ────────────────────────────────────────────
    public string? Interests { get; set; }

    // ── Involvement preferences ────────────────────────────────────────────
    // What are they open to? "volunteering", "leading-small-group", "hospitality"
    // Stored as a comma-separated list for simplicity — not used for filtering
    public string? InvolvementOpenTo { get; set; }

    // ── Spiritual journey (church-specific) ───────────────────────────────
    // Deliberately kept as free text — not an enum — to avoid reducing a person
    // to a checkbox. This data must never be used for automated scoring.
    public string? SpiritualNotes { get; set; }

    // ── Staff notes ────────────────────────────────────────────────────────
    public string? StaffPrivateNotes { get; set; }

    // ── Consent for enrichment data processing ─────────────────────────────
    // The contact must have explicitly agreed to their profile being maintained.
    // Without this flag, the profile should not be created.
    public bool DataEnrichmentConsented { get; set; } = false;
    public DateTimeOffset? ConsentedAt { get; set; }
    public string? ConsentSource { get; set; }

    public string LastUpdatedBy { get; set; } = "system";
}