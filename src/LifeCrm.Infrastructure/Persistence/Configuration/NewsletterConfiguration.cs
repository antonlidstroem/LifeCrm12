using LifeCrm.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeCrm.Infrastructure.Persistence.Configuration;

public class NewsletterConfiguration : IEntityTypeConfiguration<Newsletter>
{
    public void Configure(EntityTypeBuilder<Newsletter> b)
    {
        b.Property(n => n.HtmlBody).HasColumnType("nvarchar(max)");
        b.HasIndex(n => new { n.OrganizationId, n.Status, n.CreatedAt });
    }
}
