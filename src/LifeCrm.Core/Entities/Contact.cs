namespace LifeCrm.Core.Entities;

/// <summary>
/// Core contact entity. Only FirstName + Email are required.
/// Everything else is optional and admin-enriched asynchronously.
///
/// GDPR: Email, Phone, Address, Notes are encrypted at rest via EncryptedStringConverter.
///       EmailHash (HMAC-SHA256) is stored in plaintext for indexed lookup.
/// </summary>
public class Contact : TenantEntity
{
    // ── Required (the ONLY two fields staff must provide) ─────────────────
    public string  FirstName   { get; set; } = string.Empty;

    // Optional: single-name contacts, orgs added from a sign-up sheet are valid
    public string? LastName    { get; set; }

    /// <summary>Computed — not mapped to a DB column.</summary>
    public string  FullName    => string.IsNullOrWhiteSpace(LastName)
        ? FirstName
        : $"{FirstName} {LastName}".Trim();

    // Legacy combined column — kept during transition, not written in new code
    [Obsolete("Use FirstName and LastName. Will be removed in a future migration.")]
    public string  Name        { get; set; } = string.Empty;

    // ── Contact details (all optional, encrypted at rest where noted) ─────
    public string? Email       { get; set; }   // Encrypted
    public string? EmailHash   { get; set; }   // HMAC-SHA256 for indexed lookup (plaintext)
    public string? Phone       { get; set; }   // Encrypted
    public string? Address     { get; set; }   // Single-line. Encrypted
    public string? City        { get; set; }
    public string? Country     { get; set; }
    public string? Notes       { get; set; }   // Free text. Encrypted

    // ── Communication preferences ─────────────────────────────────────────
    public bool    EmailOptOut        { get; set; } = false;
    public DateTimeOffset? EmailOptOutAt     { get; set; }
    public string? EmailOptOutSource  { get; set; }

    // ── Source of creation ────────────────────────────────────────────────
    // "event-checkin" | "donation-form" | "manual-admin" | "csv-import" | "newsletter-signup"
    public string? Source      { get; set; }

    // ── GDPR state ────────────────────────────────────────────────────────
    public bool    IsAnonymized       { get; set; } = false;
    public DateTimeOffset? AnonymizedAt      { get; set; }

    // ── Audit ─────────────────────────────────────────────────────────────
    public string  CreatedBy   { get; set; } = "system";

    // ── Navigation ────────────────────────────────────────────────────────
    // Admin enrichment — always requires explicit Include, never lazy-loaded
    public ContactProfile?              Profile         { get; set; }

    // Core engagement
    public ICollection<Donation>        Donations       { get; set; } = new List<Donation>();
    public ICollection<Interaction>     Interactions    { get; set; } = new List<Interaction>();
    public ICollection<Document>        Documents       { get; set; } = new List<Document>();
    public ICollection<EventAttendance> Attendances     { get; set; } = new List<EventAttendance>();

    // Taxonomy
    public ICollection<ContactTag>      ContactTags     { get; set; } = new List<ContactTag>();

    // GDPR
    public ICollection<ConsentRecord>   Consents        { get; set; } = new List<ConsentRecord>();

    // Mentoring (admin-only)
    public ICollection<MentorRelationship> MentoringOthers  { get; set; } = new List<MentorRelationship>();
    public ICollection<MentorRelationship> BeingMentoredBy  { get; set; } = new List<MentorRelationship>();
}
