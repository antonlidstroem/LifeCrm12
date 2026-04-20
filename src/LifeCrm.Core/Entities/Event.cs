// src/LifeCrm.Core/Entities/Event.cs
using LifeCrm.Core.Entities;

public class Event : TenantEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EventType Type { get; set; } = EventType.General;
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public string? Location { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? CampaignId { get; set; }
    public bool IsPublished { get; set; } = false;

    public Project? Project { get; set; }
    public Campaign? Campaign { get; set; }
    public ICollection<EventAttendance> Attendances { get; set; } = new List<EventAttendance>();
}

public enum EventType
{
    General = 1,  // Sunday service, community gathering
    Training = 2,  // Workshop, conference
    Outreach = 3,  // Field mission, community event
    SmallGroup = 4,  // Cell group, Bible study
    OneOnOne = 5   // Pastoral meeting, mentoring session
}