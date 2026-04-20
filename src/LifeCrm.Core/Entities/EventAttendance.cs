// src/LifeCrm.Core/Entities/EventAttendance.cs
using LifeCrm.Core.Entities;

public class EventAttendance : TenantEntity
{
    public Guid EventId { get; set; }
    public Guid ContactId { get; set; }

    // How did we know they attended?
    // "checkin-kiosk", "manual-staff", "self-register", "import"
    public string Source { get; set; } = "manual-staff";

    // Optional: what role did they play?
    public AttendanceRole Role { get; set; } = AttendanceRole.Attendee;

    public string? Notes { get; set; }

    public Event? Event { get; set; }
    public Contact? Contact { get; set; }
}

public enum AttendanceRole
{
    Attendee = 1,
    Volunteer = 2,
    Speaker = 3,
    Leader = 4
}