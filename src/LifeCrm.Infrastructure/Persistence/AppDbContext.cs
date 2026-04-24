using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUser;
    private readonly IFieldEncryptionService? _encryption;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService? currentUser = null,
        IFieldEncryptionService? encryption = null)
        : base(options)
    {
        _currentUser = currentUser;
        _encryption = encryption;
    }

    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<ApplicationUser> Users { get; set; } = null!;
    public DbSet<Contact> Contacts { get; set; } = null!;
    public DbSet<Donation> Donations { get; set; } = null!;
    public DbSet<Campaign> Campaigns { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Interaction> Interactions { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<Newsletter> Newsletters { get; set; } = null!;
    public DbSet<NewsletterAttachment> NewsletterAttachments { get; set; } = null!;
    public DbSet<MissionReport> MissionReports { get; set; } = null!;
    public DbSet<DecisionCount> DecisionCounts { get; set; } = null!;
    public DbSet<PeopleGroupReached> PeopleGroupsReached { get; set; } = null!;
    public DbSet<PrayerPoint> PrayerPoints { get; set; } = null!;
    public DbSet<PeopleGroupSeed> PeopleGroupSeeds { get; set; } = null!;
    public DbSet<AppSettings> AppSettings { get; set; } = null!;

    // ── GDPR additions ────────────────────────────────────────────────────────
    public DbSet<ConsentRecord> ConsentRecords { get; set; } = null!;
    public DbSet<RetentionPolicy> RetentionPolicies { get; set; } = null!;
    // OutboxMessage accessed via Set<OutboxMessage>() — no DbSet property to discourage direct queries

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        static void Filter<T>(ModelBuilder b, ICurrentUserService? cu) where T : TenantEntity
            => b.Entity<T>().HasQueryFilter(
                e => !e.IsDeleted && (cu == null || !cu.IsAuthenticated || e.OrganizationId == cu.OrganizationId));

        Filter<Contact>(mb, _currentUser);
        Filter<Donation>(mb, _currentUser);
        Filter<Campaign>(mb, _currentUser);
        Filter<Project>(mb, _currentUser);
        Filter<Interaction>(mb, _currentUser);
        Filter<Document>(mb, _currentUser);
        Filter<ApplicationUser>(mb, _currentUser);
        Filter<Newsletter>(mb, _currentUser);
        Filter<NewsletterAttachment>(mb, _currentUser);
        Filter<MissionReport>(mb, _currentUser);
        Filter<DecisionCount>(mb, _currentUser);
        Filter<PeopleGroupReached>(mb, _currentUser);
        Filter<PrayerPoint>(mb, _currentUser);
        Filter<ConsentRecord>(mb, _currentUser);  // GDPR: tenant-scoped
        Filter<RetentionPolicy>(mb, _currentUser); // GDPR: tenant-scoped

        mb.Entity<PeopleGroupSeed>().HasQueryFilter(e => !e.IsDeleted);
        mb.Entity<AppSettings>().HasQueryFilter(e => !e.IsDeleted);
        mb.Entity<OutboxMessage>().HasQueryFilter(_ => true); // no filter on outbox

        // Apply configurations — pass encryption service to ContactConfiguration
        if (_encryption is not null)
            mb.ApplyConfiguration(new ContactConfiguration(_encryption));
        else
            mb.Entity<Contact>().HasIndex(c => new { c.OrganizationId, c.EmailHash });

        mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}