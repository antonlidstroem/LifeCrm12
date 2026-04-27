// src/LifeCrm.Infrastructure/Persistence/AppDbContext.cs
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    // FIX A: Store org ID and auth state as plain captured values at construction
    // time instead of holding a reference to ICurrentUserService and calling
    // through it on every query filter evaluation. EF Core cannot cache compiled
    // query plans when the filter predicate accesses a live service; captured
    // primitive values are stable and allow plan reuse.
    private readonly Guid? _currentOrgId;
    private readonly bool  _isAuthenticated;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService? currentUser = null)
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

        // Capture locals so EF Core sees stable value captures — not a
        // chain of property accesses through a service reference.
        var orgId    = _currentOrgId;
        var isAuthed = _isAuthenticated;

        void Filter<T>(ModelBuilder b) where T : TenantEntity
            => b.Entity<T>().HasQueryFilter(
                e => !e.IsDeleted
                  && (!isAuthed || orgId == null || e.OrganizationId == orgId));

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
