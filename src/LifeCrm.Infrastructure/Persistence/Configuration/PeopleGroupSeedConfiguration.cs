using LifeCrm.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

public class PeopleGroupSeedConfiguration : IEntityTypeConfiguration<PeopleGroupSeed>
{
    public void Configure(EntityTypeBuilder<PeopleGroupSeed> b)
    {
        b.HasIndex(p => p.JpCode).IsUnique();
        b.HasIndex(p => p.Name);
    }
}
