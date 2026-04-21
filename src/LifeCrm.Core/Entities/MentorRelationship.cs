using LifeCrm.Core.Attributes;
using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities;

/// <summary>
/// Admin-managed directional mentoring relationship between two contacts.
/// Contacts are NEVER shown this data — purely for staff coordination.
/// </summary>
[AdminOnly("Mentoring relationships are admin-only — contacts are not informed via the system.")]
public class MentorRelationship : TenantEntity
{
    public Guid   MentorContactId  { get; set; }
    public Guid   MenteeContactId  { get; set; }

    public MentorshipType Type     { get; set; } = MentorshipType.General;

    public DateOnly? StartDate     { get; set; }
    public DateOnly? EndDate       { get; set; }   // null = ongoing

    public string?   Notes         { get; set; }
    public bool      IsActive      { get; set; } = true;

    public Guid      CreatedByUserId { get; set; }

    public Contact?  Mentor        { get; set; }
    public Contact?  Mentee        { get; set; }
}
