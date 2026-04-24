using LifeCrm.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

public class ConsentRecordConfiguration : IEntityTypeConfiguration<ConsentRecord>
{
    public void Configure(EntityTypeBuilder<ConsentRecord> b)
    {
        b.ToTable("ConsentRecords");

        b.HasKey(cr => cr.Id);

        b.Property(cr => cr.PolicyVersion).HasMaxLength(50).IsRequired();
        b.Property(cr => cr.LegalBasis).HasMaxLength(100).IsRequired();
        b.Property(cr => cr.Channel).HasMaxLength(100);
        b.Property(cr => cr.IpAddressHash).HasMaxLength(64); // SHA-256 hex
        b.Property(cr => cr.RecordedBy).HasMaxLength(200).IsRequired();
        b.Property(cr => cr.Notes).HasMaxLength(2000);

        // Primary query pattern: latest record per (ContactId, ConsentType)
        b.HasIndex(cr => new { cr.ContactId, cr.ConsentType, cr.RecordedAt })
            .HasDatabaseName("IX_ConsentRecords_Contact_Type_Date");

        // Org-level query for admin screens
        b.HasIndex(cr => new { cr.OrganizationId, cr.RecordedAt })
            .HasDatabaseName("IX_ConsentRecords_Org_Date");

        b.HasOne(cr => cr.Contact)
            .WithMany(c => c.ConsentRecords)
            .HasForeignKey(cr => cr.ContactId)
            .OnDelete(DeleteBehavior.Cascade); // cascade with contact
    }
}