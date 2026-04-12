using LifeCrm.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

public class MissionReportConfiguration : IEntityTypeConfiguration<MissionReport>
{
    public void Configure(EntityTypeBuilder<MissionReport> b)
    {
        b.Property(r => r.HtmlBody).HasColumnType("nvarchar(max)");
        b.HasOne(r => r.Campaign).WithMany().HasForeignKey(r => r.CampaignId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(r => r.Project).WithMany().HasForeignKey(r => r.ProjectId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(r => new { r.OrganizationId, r.Status, r.ReportDate });
        b.HasIndex(r => r.CampaignId);
        b.HasIndex(r => r.ProjectId);
    }
}
