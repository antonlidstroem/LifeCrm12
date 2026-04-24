using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities;

public class Contact : TenantEntity
{
    // ── Phase 1: Split Name → FirstName + LastName ────────────────────────────
    // Name column kept for backward compatibility during migration.
    // After Phase 4: drop Name, make FullName a SQL computed column.

    /// <summary>DEPRECATED. Use FirstName + LastName. Kept for BC during migration.</summary>
    public string Name { get; set; } = string.Empty;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    /// <summary>
    /// Computed display name. Not mapped to DB — derived from FirstName/LastName when set,
    /// falls back to legacy Name otherwise.
    /// </summary>
    public string FullName =>
        (!string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName))
            ? $"{FirstName} {LastName}".Trim()
            : Name;

    public ContactType Type { get; set; } = ContactType.Individual;
    public string? Email { get; set; }

    /// <summary>
    /// Deterministic HMAC-SHA256 hash of lowercased email.
    /// Used for unique index lookups after email column is encrypted.
    /// Updated by PropertyAuditInterceptor whenever Email changes.
    /// </summary>
    public string? EmailHash { get; set; }

    public string? Phone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Tags { get; set; }
    public string? Notes { get; set; }
    public string? PrimaryContactName { get; set; }

    /// <summary>
    /// Legacy opt-out flag. In Phase 2 this becomes derived from ConsentRecords.
    /// Kept as a concrete column during transition for performance.
    /// </summary>
    public bool EmailOptOut { get; set; } = false;

    public string CreatedBy { get; set; } = "system";

    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<ConsentRecord> ConsentRecords { get; set; } = new List<ConsentRecord>();
}