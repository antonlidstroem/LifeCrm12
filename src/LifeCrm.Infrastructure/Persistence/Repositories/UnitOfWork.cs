using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;

namespace LifeCrm.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private bool _disposed;

    // ── Existing ──────────────────────────────────────────────────────────
    public IRepository<ApplicationUser>      Users                 { get; }
    public IRepository<Contact>              Contacts              { get; }
    public IRepository<Donation>             Donations             { get; }
    public ICampaignRepository               Campaigns             { get; }
    public IRepository<Project>              Projects              { get; }
    public IRepository<Interaction>          Interactions          { get; }
    public IInteractionRepository            InteractionRepo       { get; }
    public IRepository<Newsletter>           Newsletters           { get; }
    public IRepository<NewsletterAttachment> NewsletterAttachments { get; }
    public IRepository<MissionReport>        Reports               { get; }
    public IRepository<DecisionCount>        DecisionCounts        { get; }
    public IRepository<PeopleGroupReached>   PeopleGroupsReached   { get; }
    public IRepository<PrayerPoint>          PrayerPoints          { get; }
    public IRepository<PeopleGroupSeed>      PeopleGroupSeeds      { get; }

    // ── Human Growth (new) ────────────────────────────────────────────────
    public IRepository<Event>                Events                { get; }
    public IRepository<EventAttendance>      EventAttendances      { get; }
    public IRepository<Tag>                  Tags                  { get; }
    public IRepository<ContactTag>           ContactTags           { get; }
    public IRepository<ContactProfile>       ContactProfiles       { get; }
    public IRepository<MentorRelationship>   MentorRelationships   { get; }

    public UnitOfWork(AppDbContext db)
    {
        _db                   = db;
        Users                 = new GenericRepository<ApplicationUser>(db);
        Contacts              = new GenericRepository<Contact>(db);
        Donations             = new GenericRepository<Donation>(db);
        Campaigns             = new CampaignRepository(db);
        Projects              = new GenericRepository<Project>(db);
        Newsletters           = new GenericRepository<Newsletter>(db);
        NewsletterAttachments = new GenericRepository<NewsletterAttachment>(db);
        Reports               = new GenericRepository<MissionReport>(db);
        DecisionCounts        = new GenericRepository<DecisionCount>(db);
        PeopleGroupsReached   = new GenericRepository<PeopleGroupReached>(db);
        PrayerPoints          = new GenericRepository<PrayerPoint>(db);
        PeopleGroupSeeds      = new GenericRepository<PeopleGroupSeed>(db);
        var ir                = new InteractionRepository(db);
        Interactions          = ir;
        InteractionRepo       = ir;

        // Human Growth
        Events                = new GenericRepository<Event>(db);
        EventAttendances      = new GenericRepository<EventAttendance>(db);
        Tags                  = new GenericRepository<Tag>(db);
        ContactTags           = new GenericRepository<ContactTag>(db);
        ContactProfiles       = new GenericRepository<ContactProfile>(db);
        MentorRelationships   = new GenericRepository<MentorRelationship>(db);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
