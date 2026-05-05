using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    private readonly IFieldEncryptionService _enc;

    public ContactConfiguration(IFieldEncryptionService enc)
    {
        _enc = enc;
    }

    public void Configure(EntityTypeBuilder<Contact> b)
    {
        var converter = new EncryptedConverter(_enc);

        // ── PII columns — encrypted at rest ──────────────────────────────────
        // Column length 1000: AES-256-CBC of a 320-char email ≈ ~450 Base64 chars.
        b.Property(c => c.Email)
            .HasConversion(converter)
            .HasMaxLength(1000);

        b.Property(c => c.Phone)
            .HasConversion(converter)
            .HasMaxLength(500);

        b.Property(c => c.FirstName)
            .HasConversion(converter)
            .HasMaxLength(500);

        b.Property(c => c.LastName)
            .HasConversion(converter)
            .HasMaxLength(500);

        b.Property(c => c.AddressLine1)
            .HasConversion(converter)
            .HasMaxLength(1000);

        b.Property(c => c.AddressLine2)
            .HasConversion(converter)
            .HasMaxLength(1000);

        // ── EmailHash: deterministic HMAC for indexed lookup ─────────────────
        // Not encrypted — it is a one-way hash, safe to store in plain text.
        b.Property(c => c.EmailHash)
            .HasMaxLength(64) // SHA-256 hex = 64 chars
            .IsRequired(false);

        // Replace old (OrganizationId, Email) index with hash-based index.
        // The plain Email column is no longer usable for lookups after encryption.
        b.HasIndex(c => new { c.OrganizationId, c.EmailHash })
            .HasDatabaseName("IX_Contacts_OrganizationId_EmailHash");

        // ── FirstName/LastName: new nullable columns ──────────────────────────
        b.Property(c => c.FirstName).IsRequired(false);
        b.Property(c => c.LastName).IsRequired(false);

        // ── Name: kept for backward compat, will be removed in Phase 4 ───────
        b.Property(c => c.Name).HasMaxLength(500).IsRequired();

        // ── FullName is not mapped ────────────────────────────────────────────
        b.Ignore(c => c.FullName);
    }
}