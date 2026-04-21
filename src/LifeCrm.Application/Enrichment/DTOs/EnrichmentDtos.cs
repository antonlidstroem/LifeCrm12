using System.ComponentModel.DataAnnotations;
using LifeCrm.Core.Enums;

namespace LifeCrm.Application.Enrichment.DTOs;

// ── Tag DTOs ──────────────────────────────────────────────────────────────────

public record TagDto
{
    public Guid   Id       { get; init; }
    public string Name     { get; init; } = string.Empty;
    public string? Color   { get; init; }
    public string? Category { get; init; }
    public int    ContactCount { get; init; }
}

public record CreateTagRequest
{
    [Required][MaxLength(100)] public string Name     { get; init; } = string.Empty;
    [MaxLength(7)]             public string? Color   { get; init; }  // "#RRGGBB"
    [MaxLength(50)]            public string? Category { get; init; }
}

public record AssignTagRequest
{
    [Required] public Guid TagId { get; init; }
}

public record ContactTagDto
{
    public Guid   TagId       { get; init; }
    public string TagName     { get; init; } = string.Empty;
    public string? TagColor   { get; init; }
    public string? TagCategory { get; init; }
    public DateTimeOffset AssignedAt { get; init; }
}

// ── ContactProfile DTOs ───────────────────────────────────────────────────────

public record ContactProfileDto
{
    public Guid    ContactId              { get; init; }
    public string? GiftsAndTalents        { get; init; }
    public string? Interests              { get; init; }
    public string? InvolvementOpenTo      { get; init; }
    public string? SpiritualNotes         { get; init; }
    public string? StaffPrivateNotes      { get; init; }
    public bool    DataEnrichmentConsented { get; init; }
    public DateTimeOffset? ConsentedAt    { get; init; }
    public string? ConsentSource          { get; init; }
    public DateTimeOffset? LastModifiedAt { get; init; }
    public string  LastUpdatedBy          { get; init; } = string.Empty;
}

public record UpsertContactProfileRequest
{
    [MaxLength(2000)] public string? GiftsAndTalents    { get; init; }
    [MaxLength(2000)] public string? Interests          { get; init; }
    [MaxLength(2000)] public string? InvolvementOpenTo  { get; init; }
    [MaxLength(5000)] public string? SpiritualNotes     { get; init; }
    [MaxLength(5000)] public string? StaffPrivateNotes  { get; init; }
    public bool    DataEnrichmentConsented              { get; init; } = false;
    [MaxLength(100)] public string? ConsentSource       { get; init; }
}

// ── MentorRelationship DTOs ───────────────────────────────────────────────────

public record MentorRelationshipDto
{
    public Guid   Id              { get; init; }
    public Guid   MentorContactId { get; init; }
    public string MentorName      { get; init; } = string.Empty;
    public Guid   MenteeContactId { get; init; }
    public string MenteeName      { get; init; } = string.Empty;
    public MentorshipType Type    { get; init; }
    public DateOnly? StartDate    { get; init; }
    public DateOnly? EndDate      { get; init; }
    public bool   IsActive        { get; init; }
    public string? Notes          { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record CreateMentorRelationshipRequest
{
    [Required] public Guid MentorContactId { get; init; }
    [Required] public Guid MenteeContactId { get; init; }
    public MentorshipType Type { get; init; } = MentorshipType.General;
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate   { get; init; }
    [MaxLength(2000)] public string? Notes { get; init; }
}

// ── Full admin contact view ────────────────────────────────────────────────────

public record AdminContactDetailDto
{
    public Guid   ContactId    { get; init; }
    public string FullName     { get; init; } = string.Empty;
    public string? Email       { get; init; }
    public ContactProfileDto?  Profile  { get; init; }
    public IReadOnlyList<ContactTagDto>          Tags    { get; init; } = Array.Empty<ContactTagDto>();
    public IReadOnlyList<MentorRelationshipDto>  AsMentor { get; init; } = Array.Empty<MentorRelationshipDto>();
    public IReadOnlyList<MentorRelationshipDto>  AsMentee { get; init; } = Array.Empty<MentorRelationshipDto>();
}
