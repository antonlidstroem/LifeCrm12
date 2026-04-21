namespace LifeCrm.Core.Enums;

// ── Human Growth layer enums — ADD to existing Enums files ───────────────────
// These are the ONLY additions. All existing enums remain unchanged.

public enum EventType
{
    General    = 1,   // Sunday service, community gathering
    Training   = 2,   // Workshop, conference, seminar
    Outreach   = 3,   // Field mission, community event
    SmallGroup = 4,   // Cell group, Bible study, care group
    OneOnOne   = 5    // Pastoral meeting, mentoring session, counselling
}

public enum AttendanceRole
{
    Attendee  = 1,
    Volunteer = 2,
    Speaker   = 3,
    Leader    = 4
}

public enum MentorshipType
{
    General      = 1,
    Discipleship = 2,
    Leadership   = 3,
    Vocational   = 4
}
