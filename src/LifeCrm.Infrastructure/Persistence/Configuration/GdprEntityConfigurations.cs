using LifeCrm.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

// ── ConsentRecord ─────────────────────────────────────────────────────────────

public class ConsentRecordConfiguration : IEntityTypeConfiguration<ConsentRecord>
{
    public void Configure(EntityTypeBuilder<ConsentRecord> b)
    {
        b.ToTable("ConsentRecords");
        b.HasKey(r => r.Id);

        b.Property(r => r.PolicyVersion).HasMaxLength(50).IsRequired();
        b.Property(r => r.Source).HasMaxLength(100).IsRequired();
        b.Property(r => r.IpAddress).HasMaxLength(45);

        // ConsentRecords are immutable — expose IsDeleted as a column but
        // add a check constraint to prevent updates via normal EF change tracker.
        // The PropertyAuditInterceptor will still capture the initial Created event.
        b.Property(r => r.IsDeleted).HasDefaultValue(false);

        // Most queries: find latest record per contact + consent type
        b.HasIndex(r => new { r.ContactId, r.ConsentType, r.CreatedAt });

        // DSR export: all consents for a contact
        b.HasIndex(r => r.ContactId);

        b.HasOne(r => r.Contact)
         .WithMany(c => c.Consents)
         .HasForeignKey(r => r.ContactId)
         .OnDelete(DeleteBehavior.Restrict); // Consent records must survive contact anonymization
    }
}

// ── PropertyAuditLog ─────────────────────────────────────────────────────────

public class PropertyAuditLogConfiguration : IEntityTypeConfiguration<PropertyAuditLog>
{
    public void Configure(EntityTypeBuilder<PropertyAuditLog> b)
    {
        b.ToTable("PropertyAuditLogs");

        // bigint identity — high volume, no Guid overhead
        b.HasKey(l => l.Id);
        b.Property(l => l.Id).UseIdentityColumn();

        b.Property(l => l.EntityName).HasMaxLength(100).IsRequired();
        b.Property(l => l.PropertyName).HasMaxLength(100).IsRequired();
        b.Property(l => l.Action).HasMaxLength(20).IsRequired();
        b.Property(l => l.ChangedByEmail).HasMaxLength(320);

        // OldValue / NewValue can be long (JSON-serialized encrypted values)
        b.Property(l => l.OldValue).HasColumnType("nvarchar(max)");
        b.Property(l => l.NewValue).HasColumnType("nvarchar(max)");

        // Query patterns: by org + time (compliance review), by entity (record history)
        b.HasIndex(l => new { l.OrganizationId, l.ChangedAt });
        b.HasIndex(l => new { l.EntityName, l.EntityId });
    }
}

// ── AuditOutbox ───────────────────────────────────────────────────────────────

public class AuditOutboxConfiguration : IEntityTypeConfiguration<AuditOutbox>
{
    public void Configure(EntityTypeBuilder<AuditOutbox> b)
    {
        b.ToTable("AuditOutbox");
        b.HasKey(o => o.Id);
        b.Property(o => o.Id).UseIdentityColumn();
        b.Property(o => o.Payload).HasColumnType("nvarchar(max)").IsRequired();

        // Processor query: unprocessed rows ordered by insertion time
        b.HasIndex(o => new { o.IsProcessed, o.RetryCount, o.Id });
    }
}

// ── DsrExportJob ─────────────────────────────────────────────────────────────

public class DsrExportJobConfiguration : IEntityTypeConfiguration<DsrExportJob>
{
    public void Configure(EntityTypeBuilder<DsrExportJob> b)
    {
        b.ToTable("DsrExportJobs");
        b.HasKey(j => j.Id);

        b.Property(j => j.Status).HasMaxLength(20).IsRequired();
        b.Property(j => j.ExportPayload).HasColumnType("varbinary(max)");
        b.Property(j => j.ErrorMessage).HasMaxLength(2000);

        b.HasQueryFilter(j => !j.IsDeleted);

        b.HasIndex(j => new { j.OrganizationId, j.Status });
        b.HasIndex(j => j.ContactId);
    }
}
