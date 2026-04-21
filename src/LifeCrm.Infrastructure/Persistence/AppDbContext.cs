using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Encryption;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Infrastructure.Persistence;

public class AppDbContext : DbContext, IDataProtectionKeyContext
{
    private readonly ICurrentUserService? _currentUser;
    private readonly EncryptedStringConverter? _encryptor;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService? currentUser = null,
        EncryptedStringConverter? encryptor = null)
        : base(options)
    {
        _currentUser = currentUser;
        _encryptor   = encryptor;
    }

    // ── Existing DbSets ───────────────────────────────────────────────────
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

    // ── GDPR DbSets ───────────────────────────────────────────────────────
    public DbSet<ConsentRecord>        ConsentRecords        { get; set; } = null!;
    public DbSet<PropertyAuditLog>     PropertyAuditLogs     { get; set; } = null!;
    public DbSet<AuditOutbox>          AuditOutbox           { get; set; } = null!;
    public DbSet<DsrExportJob>         DsrExportJobs         { get; set; } = null!;

    // ── Human Growth DbSets (NEW) ─────────────────────────────────────────
    public DbSet<Event>                Events                { get; set; } = null!;
    public DbSet<EventAttendance>      EventAttendances      { get; set; } = null!;
    public DbSet<Tag>                  Tags                  { get; set; } = null!;
    public DbSet<ContactTag>           ContactTags           { get; set; } = null!;
    public DbSet<ContactProfile>       ContactProfiles       { get; set; } = null!;
    public DbSet<MentorRelationship>   MentorRelationships   { get; set; } = null!;

    // ── Data Protection key ring ──────────────────────────────────────────
    public DbSet<DataProtectionKey>    DataProtectionKeys    { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        static void Filter<T>(ModelBuilder b, ICurrentUserService? cu) where T : TenantEntity
            => b.Entity<T>().HasQueryFilter(e =>
                !e.IsDeleted &&
                (cu == null || !cu.IsAuthenticated || e.OrganizationId == cu.OrganizationId));

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
        // Human Growth entities
        Filter<Event>(mb, _currentUser);
        Filter<EventAttendance>(mb, _currentUser);
        Filter<Tag>(mb, _currentUser);
        Filter<ContactTag>(mb, _currentUser);

        mb.Entity<ConsentRecord>().HasQueryFilter(r => !r.IsDeleted);
        mb.Entity<PeopleGroupSeed>().HasQueryFilter(e => !e.IsDeleted);
        mb.Entity<AppSettings>().HasQueryFilter(e => !e.IsDeleted);

        // ContactProfile and MentorRelationship have their own query filters
        // defined in their EF configuration classes.

        // Apply ContactConfiguration manually (requires injected converter)
        if (_encryptor != null)
            mb.ApplyConfiguration(new ContactConfiguration(_encryptor));
        else
            mb.ApplyConfiguration(new ContactConfiguration(new NullEncryptedStringConverter()));

        mb.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly,
            t => t != typeof(ContactConfiguration));
    }
}
