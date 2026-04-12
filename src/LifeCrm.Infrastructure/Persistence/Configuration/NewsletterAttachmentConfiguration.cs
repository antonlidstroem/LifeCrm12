using LifeCrm.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

public class NewsletterAttachmentConfiguration : IEntityTypeConfiguration<NewsletterAttachment>
{
    public void Configure(EntityTypeBuilder<NewsletterAttachment> b)
    {
        b.Property(a => a.FileBytes).HasColumnType("varbinary(max)");
        b.HasIndex(a => a.NewsletterId);
        b.HasOne(a => a.Newsletter).WithMany(n => n.Attachments).HasForeignKey(a => a.NewsletterId).OnDelete(DeleteBehavior.Cascade);
    }
}
