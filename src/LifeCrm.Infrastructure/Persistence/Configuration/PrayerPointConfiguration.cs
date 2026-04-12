using LifeCrm.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

public class PrayerPointConfiguration : IEntityTypeConfiguration<PrayerPoint>
{
    public void Configure(EntityTypeBuilder<PrayerPoint> b)
    {
        b.HasOne(p => p.Report).WithMany(r => r.PrayerPoints).HasForeignKey(p => p.ReportId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(p => new { p.OrganizationId, p.Status });
    }
}
