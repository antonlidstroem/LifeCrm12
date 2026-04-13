using LifeCrm.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> b)
    {
        b.Property(c => c.BudgetGoal).HasPrecision(18, 2);

        // Campaign → Project (many-to-one, nullable FK for backward compat with existing data)
        b.HasOne(c => c.Project)
         .WithMany(p => p.Campaigns)
         .HasForeignKey(c => c.ProjectId)
         .OnDelete(DeleteBehavior.SetNull)
         .IsRequired(false);

        b.HasIndex(c => c.ProjectId);
    }
}
