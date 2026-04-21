using LifeCrm.Core.Entities;

namespace LifeCrm.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // ── Existing ──────────────────────────────────────────────────────────
    IRepository<ApplicationUser>      Users                 { get; }
    IRepository<Contact>              Contacts              { get; }
    IRepository<Donation>             Donations             { get; }
    ICampaignRepository               Campaigns             { get; }
    IRepository<Project>              Projects              { get; }
    IRepository<Interaction>          Interactions          { get; }
    IInteractionRepository            InteractionRepo       { get; }
    IRepository<Newsletter>           Newsletters           { get; }
    IRepository<NewsletterAttachment> NewsletterAttachments { get; }
    IRepository<MissionReport>        Reports               { get; }
    IRepository<DecisionCount>        DecisionCounts        { get; }
    IRepository<PeopleGroupReached>   PeopleGroupsReached   { get; }
    IRepository<PrayerPoint>          PrayerPoints          { get; }
    IRepository<PeopleGroupSeed>      PeopleGroupSeeds      { get; }

    // ── Human Growth (new) ────────────────────────────────────────────────
    IRepository<Event>                Events                { get; }
    IRepository<EventAttendance>      EventAttendances      { get; }
    IRepository<Tag>                  Tags                  { get; }
    IRepository<ContactTag>           ContactTags           { get; }
    IRepository<ContactProfile>       ContactProfiles       { get; }
    IRepository<MentorRelationship>   MentorRelationships   { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
