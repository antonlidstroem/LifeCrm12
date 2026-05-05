using LifeCrm.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("OutboxMessages");
        b.HasKey(m => m.Id);

        b.Property(m => m.Type).HasMaxLength(100).IsRequired();
        b.Property(m => m.Payload).HasColumnType("nvarchar(max)").IsRequired();
        b.Property(m => m.Error).HasMaxLength(1000);

        // Primary processor query: unprocessed, not poisoned, ordered by age
        b.HasIndex(m => new { m.ProcessedAt, m.RetryCount, m.CreatedAt })
            .HasDatabaseName("IX_OutboxMessages_Processor");
    }
}