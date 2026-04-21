using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Events.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Events.Queries;

// ── GetEvents ─────────────────────────────────────────────────────────────────

public record GetEventsQuery(PaginationParams Params) : IRequest<PagedResult<EventListDto>>;

public sealed class GetEventsHandler : IRequestHandler<GetEventsQuery, PagedResult<EventListDto>>
{
    private readonly IUnitOfWork _uow;
    public GetEventsHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<PagedResult<EventListDto>> Handle(GetEventsQuery q, CancellationToken ct)
    {
        var p     = q.Params;
        var query = _uow.Events.Query();

        if (!string.IsNullOrWhiteSpace(p.Search))
        {
            var t = p.Search.ToLower();
            query = query.Where(e => e.Title.ToLower().Contains(t)
                || (e.Location != null && e.Location.ToLower().Contains(t)));
        }

        query = p.SortAscending
            ? query.OrderByDescending(e => e.StartsAt)
            : query.OrderBy(e => e.StartsAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((p.Page - 1) * p.PageSize).Take(p.PageSize)
            .Select(e => new EventListDto
            {
                Id           = e.Id,
                Title        = e.Title,
                Type         = e.Type,
                StartsAt     = e.StartsAt,
                EndsAt       = e.EndsAt,
                Location     = e.Location,
                IsPublished  = e.IsPublished,
                AttendeeCount = e.Attendances.Count(a => !a.IsDeleted),
                ProjectName  = e.Project != null ? e.Project.Name : null,
                CreatedAt    = e.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<EventListDto>
        {
            Items = items, Page = p.Page, PageSize = p.PageSize, TotalCount = total
        };
    }
}

// ── GetEventById ──────────────────────────────────────────────────────────────

public record GetEventByIdQuery(Guid EventId) : IRequest<EventDto>;

public sealed class GetEventByIdHandler : IRequestHandler<GetEventByIdQuery, EventDto>
{
    private readonly IUnitOfWork _uow;
    public GetEventByIdHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<EventDto> Handle(GetEventByIdQuery q, CancellationToken ct)
    {
        var ev = await _uow.Events.Query()
            .Include(e => e.Attendances.Where(a => !a.IsDeleted))
                .ThenInclude(a => a.Contact)
            .Include(e => e.Project)
            .Include(e => e.Campaign)
            .FirstOrDefaultAsync(e => e.Id == q.EventId, ct)
            ?? throw new NotFoundException(nameof(Event), q.EventId);

        return new EventDto
        {
            Id           = ev.Id,
            Title        = ev.Title,
            Description  = ev.Description,
            Type         = ev.Type,
            StartsAt     = ev.StartsAt,
            EndsAt       = ev.EndsAt,
            Location     = ev.Location,
            IsPublished  = ev.IsPublished,
            AttendeeCount = ev.Attendances.Count,
            ProjectId    = ev.ProjectId,
            ProjectName  = ev.Project?.Name,
            CampaignId   = ev.CampaignId,
            CampaignName = ev.Campaign?.Name,
            CreatedAt    = ev.CreatedAt,
            Attendances  = ev.Attendances.Select(a => new AttendanceDto
            {
                Id           = a.Id,
                ContactId    = a.ContactId,
                ContactName  = a.Contact?.FullName ?? string.Empty,
                ContactEmail = a.Contact?.Email,
                Role         = a.Role,
                Source       = a.Source,
                Notes        = a.Notes,
                CheckedInAt  = a.CreatedAt
            }).ToList().AsReadOnly()
        };
    }
}

// ── GetContactEngagement ──────────────────────────────────────────────────────

public record GetContactEngagementQuery(Guid ContactId) : IRequest<ContactEngagementDto>;

public sealed class GetContactEngagementHandler
    : IRequestHandler<GetContactEngagementQuery, ContactEngagementDto>
{
    private readonly IUnitOfWork _uow;
    public GetContactEngagementHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<ContactEngagementDto> Handle(GetContactEngagementQuery q, CancellationToken ct)
    {
        var attendances = await _uow.EventAttendances.Query()
            .Where(a => a.ContactId == q.ContactId)
            .Include(a => a.Event)
            .OrderByDescending(a => a.Event!.StartsAt)
            .ToListAsync(ct);

        var donations = await _uow.Donations.Query()
            .Where(d => d.ContactId == q.ContactId)
            .OrderByDescending(d => d.Date)
            .ToListAsync(ct);

        var interactionCount = await _uow.Interactions
            .CountAsync(i => i.ContactId == q.ContactId, ct);

        var totalDonated    = donations.Sum(d => d.Amount);
        var lastEventAt     = attendances.FirstOrDefault()?.Event?.StartsAt;
        var lastDonationAt  = donations.FirstOrDefault() is { } d2
            ? (DateTimeOffset?)new DateTimeOffset(d2.Date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            : null;

        var segment = ComputeSegment(attendances.Count + donations.Count, lastEventAt);

        var recentEvents = attendances.Take(5).Select(a => new RecentEventDto
        {
            EventId    = a.EventId,
            EventTitle = a.Event?.Title ?? string.Empty,
            Type       = a.Event?.Type ?? Core.Enums.EventType.General,
            StartsAt   = a.Event?.StartsAt ?? DateTimeOffset.MinValue,
            Role       = a.Role
        }).ToList().AsReadOnly();

        return new ContactEngagementDto
        {
            ContactId         = q.ContactId,
            TotalEvents       = attendances.Count,
            TotalDonations    = donations.Count,
            TotalInteractions = interactionCount,
            TotalDonated      = totalDonated,
            LastEventAt       = lastEventAt,
            LastDonationAt    = lastDonationAt,
            EngagementSegment = segment,
            RecentEvents      = recentEvents
        };
    }

    private static string ComputeSegment(int totalEngagements, DateTimeOffset? lastEngagement)
    {
        var daysSinceLast = lastEngagement.HasValue
            ? (DateTimeOffset.UtcNow - lastEngagement.Value).TotalDays
            : double.MaxValue;

        return (totalEngagements, daysSinceLast) switch
        {
            (0, _)        => "Lapsed",
            (<= 1, _)     => "New",
            (_, > 180)    => "Lapsed",
            (_, > 60)     => "AtRisk",
            (>= 6, <= 60) => "Core",
            _             => "Growing"
        };
    }
}
