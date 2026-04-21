using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities;

/// <summary>
/// An event, service, gathering, or activity tracked by the organisation.
/// Primary engagement entry point — contacts are linked via EventAttendance.
/// </summary>
public class Event : TenantEntity
{
    public string     Title        { get; set; } = string.Empty;
    public string?    Description  { get; set; }
    public EventType  Type         { get; set; } = EventType.General;
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset? EndsAt  { get; set; }
    public string?    Location     { get; set; }
    public bool       IsPublished  { get; set; } = false;

    // Optional linkage for fundraising / mission events
    public Guid?      ProjectId    { get; set; }
    public Guid?      CampaignId   { get; set; }

    public string     CreatedBy    { get; set; } = "system";

    public Project?   Project      { get; set; }
    public Campaign?  Campaign     { get; set; }
    public ICollection<EventAttendance> Attendances { get; set; } = new List<EventAttendance>();
}

/// <summary>
/// Records that a specific contact attended a specific event.
/// The primary engagement data point. Created via check-in panel, import, or manual entry.
/// </summary>
public class EventAttendance : TenantEntity
{
    public Guid   EventId   { get; set; }
    public Guid   ContactId { get; set; }

    // "manual-staff" | "checkin-panel" | "self-register" | "import"
    public string Source    { get; set; } = "manual-staff";

    public AttendanceRole Role { get; set; } = AttendanceRole.Attendee;
    public string? Notes       { get; set; }

    public Event?   Event   { get; set; }
    public Contact? Contact { get; set; }
}
