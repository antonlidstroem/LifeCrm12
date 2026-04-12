using LifeCrm.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

public class PeopleGroupReachedConfiguration : IEntityTypeConfiguration<PeopleGroupReached>
{
    public void Configure(EntityTypeBuilder<PeopleGroupReached> b)
    {
        b.HasOne(p => p.Report).WithMany(r => r.PeopleGroups).HasForeignKey(p => p.ReportId).OnDelete(DeleteBehavior.Cascade);
    }
}
