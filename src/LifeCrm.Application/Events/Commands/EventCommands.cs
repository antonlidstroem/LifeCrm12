using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Events.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Events.Commands;

// ── Create ────────────────────────────────────────────────────────────────────

public record CreateEventCommand(CreateEventRequest Request) : IRequest<Guid>;

public sealed class CreateEventHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;

    public CreateEventHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

    public async Task<Guid> Handle(CreateEventCommand cmd, CancellationToken ct)
    {
        var orgId = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
        var r = cmd.Request;

        var ev = new Event
        {
            Id          = Guid.NewGuid(),
            OrganizationId = orgId,
            Title       = r.Title.Trim(),
            Description = r.Description?.Trim(),
            Type        = r.Type,
            StartsAt    = r.StartsAt,
            EndsAt      = r.EndsAt,
            Location    = r.Location?.Trim(),
            IsPublished = r.IsPublished,
            ProjectId   = r.ProjectId,
            CampaignId  = r.CampaignId,
            CreatedBy   = _cu.UserId?.ToString() ?? "system"
        };

        await _uow.Events.AddAsync(ev, ct);
        await _uow.SaveChangesAsync(ct);
        return ev.Id;
    }
}

// ── Update ────────────────────────────────────────────────────────────────────

public record UpdateEventCommand(UpdateEventRequest Request) : IRequest<Unit>;

public sealed class UpdateEventHandler : IRequestHandler<UpdateEventCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public UpdateEventHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(UpdateEventCommand cmd, CancellationToken ct)
    {
        var ev = await _uow.Events.GetByIdAsync(cmd.Request.Id, ct)
            ?? throw new NotFoundException(nameof(Event), cmd.Request.Id);
        var r = cmd.Request;

        ev.Title       = r.Title.Trim();
        ev.Description = r.Description?.Trim();
        ev.Type        = r.Type;
        ev.StartsAt    = r.StartsAt;
        ev.EndsAt      = r.EndsAt;
        ev.Location    = r.Location?.Trim();
        ev.IsPublished = r.IsPublished;
        ev.ProjectId   = r.ProjectId;
        ev.CampaignId  = r.CampaignId;

        _uow.Events.Update(ev);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

// ── Delete ────────────────────────────────────────────────────────────────────

public record DeleteEventCommand(Guid EventId) : IRequest<Unit>;

public sealed class DeleteEventHandler : IRequestHandler<DeleteEventCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public DeleteEventHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(DeleteEventCommand cmd, CancellationToken ct)
    {
        var ev = await _uow.Events.GetByIdAsync(cmd.EventId, ct)
            ?? throw new NotFoundException(nameof(Event), cmd.EventId);
        _uow.Events.Delete(ev);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

// ── CheckIn ───────────────────────────────────────────────────────────────────

public record CheckInCommand(Guid EventId, CheckInRequest Request) : IRequest<CheckInResult>;

public sealed class CheckInHandler : IRequestHandler<CheckInCommand, CheckInResult>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;
    private readonly IEmailHashService _emailHash;

    public CheckInHandler(IUnitOfWork uow, ICurrentUserService cu, IEmailHashService emailHash)
    {
        _uow = uow; _cu = cu; _emailHash = emailHash;
    }

    public async Task<CheckInResult> Handle(CheckInCommand cmd, CancellationToken ct)
    {
        var orgId = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
        var r = cmd.Request;

        _ = await _uow.Events.GetByIdAsync(cmd.EventId, ct)
            ?? throw new NotFoundException(nameof(Event), cmd.EventId);

        Guid contactId;
        string contactName;
        bool isNew = false;

        if (r.ContactId.HasValue)
        {
            // Path A — existing contact
            var contact = await _uow.Contacts.GetByIdAsync(r.ContactId.Value, ct)
                ?? throw new NotFoundException(nameof(Contact), r.ContactId.Value);
            contactId   = contact.Id;
            contactName = contact.FullName;
        }
        else
        {
            // Path B — create minimal contact on the fly
            if (string.IsNullOrWhiteSpace(r.FirstName))
                throw new ValidationException("FirstName", "First name is required when checking in a new contact.");

            var normalizedEmail = r.Email?.Trim().ToLowerInvariant();

            // Find-or-create by email hash to prevent duplicates
            Contact? existing = null;
            if (normalizedEmail != null)
            {
                var hash = _emailHash.Hash(normalizedEmail);
                existing = _uow.Contacts.Query()
                    .FirstOrDefault(c => c.EmailHash == hash);
            }

            if (existing != null)
            {
                contactId   = existing.Id;
                contactName = existing.FullName;
            }
            else
            {
                var normalizedEmail2 = r.Email?.Trim().ToLowerInvariant();
                var newContact = new Contact
                {
                    Id             = Guid.NewGuid(),
                    OrganizationId = orgId,
                    FirstName      = r.FirstName.Trim(),
                    LastName       = r.LastName?.Trim(),
#pragma warning disable CS0618
                    Name           = $"{r.FirstName.Trim()} {r.LastName?.Trim()}".Trim(),
#pragma warning restore CS0618
                    Email          = normalizedEmail2,
                    EmailHash      = normalizedEmail2 != null ? _emailHash.Hash(normalizedEmail2) : null,
                    Source         = "event-checkin",
                    CreatedBy      = _cu.UserId?.ToString() ?? "system"
                };
                await _uow.Contacts.AddAsync(newContact, ct);
                contactId   = newContact.Id;
                contactName = newContact.FullName;
                isNew       = true;
            }
        }

        // Prevent duplicate attendance on the same event
        var alreadyCheckedIn = _uow.EventAttendances.Query()
            .Any(a => a.EventId == cmd.EventId && a.ContactId == contactId);

        if (alreadyCheckedIn)
            throw new ConflictException($"{contactName} is already checked in to this event.");

        var attendance = new EventAttendance
        {
            Id             = Guid.NewGuid(),
            OrganizationId = orgId,
            EventId        = cmd.EventId,
            ContactId      = contactId,
            Role           = r.Role,
            Source         = r.Source,
            Notes          = r.Notes?.Trim()
        };

        await _uow.EventAttendances.AddAsync(attendance, ct);
        await _uow.SaveChangesAsync(ct);

        return new CheckInResult
        {
            AttendanceId = attendance.Id,
            ContactId    = contactId,
            ContactName  = contactName,
            IsNewContact = isNew,
            Message      = isNew
                ? $"{contactName} checked in (new contact created)."
                : $"{contactName} checked in."
        };
    }
}

// ── Remove attendance ─────────────────────────────────────────────────────────

public record RemoveAttendanceCommand(Guid AttendanceId) : IRequest<Unit>;

public sealed class RemoveAttendanceHandler : IRequestHandler<RemoveAttendanceCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public RemoveAttendanceHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(RemoveAttendanceCommand cmd, CancellationToken ct)
    {
        var att = await _uow.EventAttendances.GetByIdAsync(cmd.AttendanceId, ct)
            ?? throw new NotFoundException(nameof(EventAttendance), cmd.AttendanceId);
        _uow.EventAttendances.Delete(att);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
