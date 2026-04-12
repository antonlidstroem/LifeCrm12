using LifeCrm.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

public class AppSettingsConfiguration : IEntityTypeConfiguration<AppSettings>
{
    public void Configure(EntityTypeBuilder<AppSettings> b)
    {
        b.ToTable("AppSettings");
        b.HasIndex(s => s.Key).IsUnique();
        b.Property(s => s.Key).HasMaxLength(200).IsRequired();
        b.Property(s => s.Value).HasMaxLength(2000).IsRequired();
    }
}
