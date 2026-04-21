using LifeCrm.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

// ── Event ─────────────────────────────────────────────────────────────────────

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> b)
    {
        b.ToTable("Events");
        b.Property(e => e.Title).HasMaxLength(200).IsRequired();
        b.Property(e => e.Location).HasMaxLength(200);

        b.HasOne(e => e.Project).WithMany()
            .HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(e => e.Campaign).WithMany()
            .HasForeignKey(e => e.CampaignId).OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(e => new { e.OrganizationId, e.StartsAt });
        b.HasIndex(e => e.ProjectId);
    }
}

// ── EventAttendance ───────────────────────────────────────────────────────────

public class EventAttendanceConfiguration : IEntityTypeConfiguration<EventAttendance>
{
    public void Configure(EntityTypeBuilder<EventAttendance> b)
    {
        b.ToTable("EventAttendances");
        b.Property(a => a.Source).HasMaxLength(50).IsRequired();
        b.Property(a => a.Notes).HasMaxLength(500);

        b.HasOne(a => a.Event).WithMany(e => e.Attendances)
            .HasForeignKey(a => a.EventId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(a => a.Contact).WithMany(c => c.Attendances)
            .HasForeignKey(a => a.ContactId).OnDelete(DeleteBehavior.Restrict);

        // Prevent duplicate attendance for the same contact+event
        b.HasIndex(a => new { a.EventId, a.ContactId }).IsUnique();
        b.HasIndex(a => a.ContactId);
    }
}

// ── Tag ───────────────────────────────────────────────────────────────────────

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> b)
    {
        b.ToTable("Tags");
        b.Property(t => t.Name).HasMaxLength(100).IsRequired();
        b.Property(t => t.Color).HasMaxLength(7);
        b.Property(t => t.Category).HasMaxLength(50);

        // Unique name per org
        b.HasIndex(t => new { t.OrganizationId, t.Name }).IsUnique();
    }
}

// ── ContactTag ────────────────────────────────────────────────────────────────

public class ContactTagConfiguration : IEntityTypeConfiguration<ContactTag>
{
    public void Configure(EntityTypeBuilder<ContactTag> b)
    {
        b.ToTable("ContactTags");

        b.HasOne(ct => ct.Tag).WithMany(t => t.ContactTags)
            .HasForeignKey(ct => ct.TagId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(ct => ct.Contact).WithMany(c => c.ContactTags)
            .HasForeignKey(ct => ct.ContactId).OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(ct => new { ct.ContactId, ct.TagId }).IsUnique();
        b.HasIndex(ct => ct.TagId);
    }
}

// ── ContactProfile ────────────────────────────────────────────────────────────

public class ContactProfileConfiguration : IEntityTypeConfiguration<ContactProfile>
{
    public void Configure(EntityTypeBuilder<ContactProfile> b)
    {
        b.ToTable("ContactProfiles");

        // Free-text fields — no max length on spiritual/staff notes
        b.Property(p => p.GiftsAndTalents).HasMaxLength(2000);
        b.Property(p => p.Interests).HasMaxLength(2000);
        b.Property(p => p.InvolvementOpenTo).HasMaxLength(2000);
        b.Property(p => p.SpiritualNotes).HasColumnType("nvarchar(max)");
        b.Property(p => p.StaffPrivateNotes).HasColumnType("nvarchar(max)");
        b.Property(p => p.ConsentSource).HasMaxLength(100);
        b.Property(p => p.LastUpdatedBy).HasMaxLength(100);

        // 1:1 — ContactId is unique FK
        b.HasOne(p => p.Contact).WithOne(c => c.Profile)
            .HasForeignKey<ContactProfile>(p => p.ContactId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(p => p.ContactId).IsUnique();

        // No global query filter needed — endpoint authorization handles visibility
        b.HasQueryFilter(p => !p.IsDeleted);
    }
}

// ── MentorRelationship ────────────────────────────────────────────────────────

public class MentorRelationshipConfiguration : IEntityTypeConfiguration<MentorRelationship>
{
    public void Configure(EntityTypeBuilder<MentorRelationship> b)
    {
        b.ToTable("MentorRelationships");
        b.Property(r => r.Notes).HasMaxLength(2000);

        b.HasOne(r => r.Mentor).WithMany(c => c.MentoringOthers)
            .HasForeignKey(r => r.MentorContactId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(r => r.Mentee).WithMany(c => c.BeingMentoredBy)
            .HasForeignKey(r => r.MenteeContactId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(r => new { r.OrganizationId, r.IsActive });
        b.HasIndex(r => r.MentorContactId);
        b.HasIndex(r => r.MenteeContactId);

        b.HasQueryFilter(r => !r.IsDeleted);
    }
}
