// src/LifeCrm.Core/Entities/MentorRelationship.cs
//
// Directional: one Mentor → many Mentees (but stored as individual pairs).
// Admin creates and manages these. Contacts are never aware of this data
// unless the organization explicitly tells them.
using LifeCrm.Core.Entities;

public class MentorRelationship : TenantEntity
{
    public Guid MentorContactId { get; set; }
    public Guid MenteeContactId { get; set; }

    public MentorshipType Type { get; set; } = MentorshipType.General;

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }   // null = ongoing

    public string? Notes { get; set; }   // Admin-only context
    public bool IsActive { get; set; } = true;

    public Guid CreatedByUserId { get; set; }

    public Contact? Mentor { get; set; }
    public Contact? Mentee { get; set; }
}

public enum MentorshipType
{
    General = 1,
    Discipleship = 2,
    Leadership = 3,
    Vocational = 4
}