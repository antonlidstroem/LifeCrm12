using LifeCrm.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

public class RetentionPolicyConfiguration : IEntityTypeConfiguration<RetentionPolicy>
{
    public void Configure(EntityTypeBuilder<RetentionPolicy> b)
    {
        b.ToTable("RetentionPolicies");
        b.HasKey(p => p.Id);
        b.Property(p => p.EntityName).HasMaxLength(100).IsRequired();
        b.HasIndex(p => new { p.OrganizationId, p.EntityName, p.IsActive })
            .HasDatabaseName("IX_RetentionPolicies_Org_Entity");
    }
}