using LifeCrm.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

public class DecisionCountConfiguration : IEntityTypeConfiguration<DecisionCount>
{
    public void Configure(EntityTypeBuilder<DecisionCount> b)
    {
        b.HasOne(d => d.Report).WithMany(r => r.Decisions).HasForeignKey(d => d.ReportId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(d => new { d.ReportId, d.DecisionType }).IsUnique();
    }
}
