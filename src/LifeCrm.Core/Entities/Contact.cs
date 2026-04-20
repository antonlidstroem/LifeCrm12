// src/LifeCrm.Core/Entities/Contact.cs
// PRINCIPLE: Only FirstName + Email are enforced required.
// Everything else is optional and admin-added.
using LifeCrm.Core.Entities;

public class Contact : TenantEntity
{
    // ── Required (the only two fields a staff member must provide) ────────
    public string FirstName { get; set; } = string.Empty;

    // LastName intentionally optional — single-name individuals, organizations,
    // or contacts added from a first-name-only sign-up sheet must be accommodatable.
    public string? LastName { get; set; }

    // Email is the deduplication key and the communication channel.
    // Encrypted at rest. EmailHash stored for indexed lookup.
    public string? Email { get; set; }
    public string? EmailHash { get; set; }

    // ── Optional contact details (all nullable, admin-enriched) ───────────
    public string? Phone { get; set; }
    public string? Address { get; set; }   // Simplified: one address line
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? Notes { get; set; }   // Free text, admin-only

    // ── Communication preferences ─────────────────────────────────────────
    public bool EmailOptOut { get; set; } = false;
    public DateTimeOffset? EmailOptOutAt { get; set; }
    public string? EmailOptOutSource { get; set; }

    // ── GDPR state ────────────────────────────────────────────────────────
    public bool IsAnonymized { get; set; } = false;
    public DateTimeOffset? AnonymizedAt { get; set; }

    // ── Source of creation (for segmentation without profiling) ───────────
    // Where did this contact enter the system? "event-checkin", "donation-form",
    // "manual-admin", "csv-import", "newsletter-signup"
    public string? Source { get; set; }

    public string CreatedBy { get; set; } = "system";

    // ── Navigation ────────────────────────────────────────────────────────
    public ContactProfile? Profile { get; set; }  // admin-only, nullable
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
    public ICollection<EventAttendance> Attendances { get; set; } = new List<EventAttendance>();
    public ICollection<ConsentRecord> Consents { get; set; } = new List<ConsentRecord>();
    public ICollection<ContactTag> Tags { get; set; } = new List<ContactTag>();
}