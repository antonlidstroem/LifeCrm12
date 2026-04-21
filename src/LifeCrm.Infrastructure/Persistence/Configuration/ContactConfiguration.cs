using LifeCrm.Core.Entities;
using LifeCrm.Infrastructure.Encryption;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    private readonly EncryptedStringConverter _encryptor;

    public ContactConfiguration(EncryptedStringConverter encryptor)
    {
        _encryptor = encryptor;
    }

    public void Configure(EntityTypeBuilder<Contact> b)
    {
        // ── Name columns ──────────────────────────────────────────────────────
        b.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
        b.Property(c => c.LastName).HasMaxLength(100).IsRequired();

        // Legacy Name column — retained during transition window
        b.Property(c => c.Name).HasMaxLength(200).IsRequired(false);

        // FullName is a computed C# property — do not map to DB
        b.Ignore(c => c.FullName);

        // ── Encrypted PII fields ──────────────────────────────────────────────
        // These fields cannot be used in SQL WHERE clauses after encryption.
        // Use EmailHash for email-based lookups.
        b.Property(c => c.Email)
            .HasConversion(_encryptor)
            .HasMaxLength(700)     // encrypted strings are longer than plaintext
            .IsRequired(false);

        b.Property(c => c.Phone)
            .HasConversion(_encryptor)
            .HasMaxLength(300)
            .IsRequired(false);

        b.Property(c => c.AddressLine1)
            .HasConversion(_encryptor)
            .HasMaxLength(700)
            .IsRequired(false);

        b.Property(c => c.Notes)
            .HasConversion(_encryptor)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        // ── EmailHash — indexed deterministic lookup ───────────────────────────
        b.Property(c => c.EmailHash).HasMaxLength(64).IsRequired(false);

        // Replace the old OrganizationId+Email index with OrganizationId+EmailHash
        // because Email is now encrypted and cannot be indexed directly.
        b.HasIndex(c => new { c.OrganizationId, c.EmailHash });

        // ── GDPR state columns ────────────────────────────────────────────────
        b.Property(c => c.EmailOptOutSource).HasMaxLength(100).IsRequired(false);
        b.Property(c => c.IsAnonymized).HasDefaultValue(false);

        // ── Consent navigation ────────────────────────────────────────────────
        b.HasMany(c => c.Consents)
         .WithOne(r => r.Contact)
         .HasForeignKey(r => r.ContactId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
