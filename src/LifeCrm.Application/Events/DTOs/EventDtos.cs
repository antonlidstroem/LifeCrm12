using System.ComponentModel.DataAnnotations;
using LifeCrm.Core.Enums;

namespace LifeCrm.Application.Events.DTOs;

// ── Event DTOs ────────────────────────────────────────────────────────────────

public record EventListDto
{
    public Guid   Id          { get; init; }
    public string Title       { get; init; } = string.Empty;
    public EventType Type     { get; init; }
    public DateTimeOffset StartsAt { get; init; }
    public DateTimeOffset? EndsAt  { get; init; }
    public string? Location   { get; init; }
    public bool   IsPublished { get; init; }
    public int    AttendeeCount { get; init; }
    public string? ProjectName  { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record EventDto : EventListDto
{
    public string? Description { get; init; }
    public Guid?   ProjectId   { get; init; }
    public Guid?   CampaignId  { get; init; }
    public string? CampaignName { get; init; }
    public IReadOnlyList<AttendanceDto> Attendances { get; init; } = Array.Empty<AttendanceDto>();
}

public record CreateEventRequest
{
    [Required][MaxLength(200)] public string Title       { get; init; } = string.Empty;
    [MaxLength(2000)]          public string? Description { get; init; }
    public EventType  Type      { get; init; } = EventType.General;
    [Required] public DateTimeOffset StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    [MaxLength(200)] public string? Location  { get; init; }
    public bool  IsPublished    { get; init; } = false;
    public Guid? ProjectId      { get; init; }
    public Guid? CampaignId     { get; init; }
}

public record UpdateEventRequest : CreateEventRequest
{
    [Required] public Guid Id { get; init; }
}

// ── Attendance DTOs ───────────────────────────────────────────────────────────

public record AttendanceDto
{
    public Guid   Id          { get; init; }
    public Guid   ContactId   { get; init; }
    public string ContactName { get; init; } = string.Empty;
    public string? ContactEmail { get; init; }
    public AttendanceRole Role { get; init; }
    public string Source      { get; init; } = string.Empty;
    public string? Notes      { get; init; }
    public DateTimeOffset CheckedInAt { get; init; }
}

/// <summary>
/// Check-in request. Either ContactId (existing) or FirstName+Email (new contact).
/// One endpoint handles both paths — zero friction.
/// </summary>
public record CheckInRequest
{
    // Path A: existing contact
    public Guid? ContactId    { get; init; }

    // Path B: new contact (minimum required)
    [MaxLength(100)] public string? FirstName { get; init; }
    [MaxLength(100)] public string? LastName  { get; init; }
    [EmailAddress][MaxLength(320)] public string? Email { get; init; }

    public AttendanceRole Role { get; init; } = AttendanceRole.Attendee;
    public string Source       { get; init; } = "checkin-panel";
    [MaxLength(500)] public string? Notes { get; init; }
}

public record CheckInResult
{
    public Guid   AttendanceId { get; init; }
    public Guid   ContactId    { get; init; }
    public string ContactName  { get; init; } = string.Empty;
    public bool   IsNewContact { get; init; }
    public string Message      { get; init; } = string.Empty;
}

// ── Engagement summary ────────────────────────────────────────────────────────

public record ContactEngagementDto
{
    public Guid   ContactId        { get; init; }
    public int    TotalEvents      { get; init; }
    public int    TotalDonations   { get; init; }
    public int    TotalInteractions { get; init; }
    public decimal TotalDonated    { get; init; }
    public DateTimeOffset? LastEventAt    { get; init; }
    public DateTimeOffset? LastDonationAt { get; init; }
    public string EngagementSegment { get; init; } = "New";
    public IReadOnlyList<RecentEventDto> RecentEvents { get; init; } = Array.Empty<RecentEventDto>();
}

public record RecentEventDto
{
    public Guid   EventId    { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public EventType Type    { get; init; }
    public DateTimeOffset StartsAt { get; init; }
    public AttendanceRole Role { get; init; }
}
