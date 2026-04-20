// src/LifeCrm.Infrastructure/Persistence/Configuration/ContactConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> b)
    {
        b.HasIndex(c => new { c.OrganizationId, c.Email });

        // New columns
        b.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
        b.Property(c => c.LastName).HasMaxLength(100).IsRequired();

        // Keep Name during transition — NOT computed in DB, populated by migration
        b.Property(c => c.Name).HasMaxLength(200).IsRequired(false);

        // FullName is a C# computed property — do not map to DB
        b.Ignore(c => c.FullName);

        // Encrypt sensitive fields (see §5)
        b.Property(c => c.Email).HasConversion<EncryptedStringConverter>();
        b.Property(c => c.Phone).HasConversion<EncryptedStringConverter>();
        b.Property(c => c.AddressLine1).HasConversion<EncryptedStringConverter>();
        b.Property(c => c.Notes).HasConversion<EncryptedStringConverter>();

        b.HasMany(c => c.Consents)
         .WithOne(r => r.Contact)
         .HasForeignKey(r => r.ContactId)
         .OnDelete(DeleteBehavior.Restrict); // Consent records survive contact anonymization
    }
}