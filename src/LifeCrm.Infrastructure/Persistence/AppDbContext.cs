using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    // FIX A: Store the org ID as a plain value at construction time instead of
    // capturing ICurrentUserService as a closure. EF Core can then cache compiled
    // query plans because the filter predicate is stable across executions.
    // The previous pattern (reading through ICurrentUserService on every evaluation)
    // prevented query plan caching and caused re-compilation on every query.
    private readonly Guid?  _currentOrgId;
    private readonly bool   _isAuthenticated;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService? currentUser = null)
        : base(options)
    {
        _currentOrgId    = currentUser?.OrganizationId;
        _isAuthenticated = currentUser?.IsAuthenticated ?? false;
    }

    public DbSet<Organization>         Organizations         { get; set; } = null!;
    public DbSet<ApplicationUser>      Users                 { get; set; } = null!;
    public DbSet<Contact>              Contacts              { get; set; } = null!;
    public DbSet<Donation>             Donations             { get; set; } = null!;
    public DbSet<Campaign>             Campaigns             { get; set; } = null!;
    public DbSet<Project>              Projects              { get; set; } = null!;
    public DbSet<Interaction>          Interactions          { get; set; } = null!;
    public DbSet<Document>             Documents             { get; set; } = null!;
    public DbSet<AuditLog>             AuditLogs             { get; set; } = null!;
    public DbSet<Newsletter>           Newsletters           { get; set; } = null!;
    public DbSet<NewsletterAttachment> NewsletterAttachments { get; set; } = null!;
    public DbSet<MissionReport>        MissionReports        { get; set; } = null!;
    public DbSet<DecisionCount>        DecisionCounts        { get; set; } = null!;
    public DbSet<PeopleGroupReached>   PeopleGroupsReached   { get; set; } = null!;
    public DbSet<PrayerPoint>          PrayerPoints          { get; set; } = null!;
    public DbSet<PeopleGroupSeed>      PeopleGroupSeeds      { get; set; } = null!;
    public DbSet<AppSettings>          AppSettings           { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // FIX A: Capture the org values into local variables so EF Core sees
        // simple value captures, not a service-call chain. This allows the
        // query compiler to parameterise the filter and reuse cached plans.
        var orgId    = _currentOrgId;
        var isAuthed = _isAuthenticated;

        void Filter<T>(ModelBuilder b) where T : TenantEntity
            => b.Entity<T>().HasQueryFilter(
                e => !e.IsDeleted && (!isAuthed || orgId == null || e.OrganizationId == orgId));

        Filter<Contact>(mb);
        Filter<Donation>(mb);
        Filter<Campaign>(mb);
        Filter<Project>(mb);
        Filter<Interaction>(mb);
        Filter<Document>(mb);
        Filter<ApplicationUser>(mb);
        Filter<Newsletter>(mb);
        Filter<NewsletterAttachment>(mb);
        Filter<MissionReport>(mb);
        Filter<DecisionCount>(mb);
        Filter<PeopleGroupReached>(mb);
        Filter<PrayerPoint>(mb);

        mb.Entity<PeopleGroupSeed>().HasQueryFilter(e => !e.IsDeleted);
        mb.Entity<AppSettings>().HasQueryFilter(e => !e.IsDeleted);

        mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
