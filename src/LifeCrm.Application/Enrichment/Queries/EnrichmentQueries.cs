using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Enrichment.Commands;
using LifeCrm.Application.Enrichment.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Enrichment.Queries;

// ── Tags ──────────────────────────────────────────────────────────────────────

public record GetTagsQuery : IRequest<IReadOnlyList<TagDto>>;

public sealed class GetTagsHandler : IRequestHandler<GetTagsQuery, IReadOnlyList<TagDto>>
{
    private readonly IUnitOfWork _uow;
    public GetTagsHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<IReadOnlyList<TagDto>> Handle(GetTagsQuery q, CancellationToken ct)
    {
        return await _uow.Tags.Query()
            .OrderBy(t => t.Category).ThenBy(t => t.Name)
            .Select(t => new TagDto
            {
                Id           = t.Id,
                Name         = t.Name,
                Color        = t.Color,
                Category     = t.Category,
                ContactCount = t.ContactTags.Count(ct2 => !ct2.IsDeleted)
            })
            .ToListAsync(ct);
    }
}

public record GetContactTagsQuery(Guid ContactId) : IRequest<IReadOnlyList<ContactTagDto>>;

public sealed class GetContactTagsHandler : IRequestHandler<GetContactTagsQuery, IReadOnlyList<ContactTagDto>>
{
    private readonly IUnitOfWork _uow;
    public GetContactTagsHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<IReadOnlyList<ContactTagDto>> Handle(GetContactTagsQuery q, CancellationToken ct)
    {
        return await _uow.ContactTags.Query()
            .Where(ct2 => ct2.ContactId == q.ContactId)
            .Include(ct2 => ct2.Tag)
            .OrderBy(ct2 => ct2.Tag!.Name)
            .Select(ct2 => new ContactTagDto
            {
                TagId       = ct2.TagId,
                TagName     = ct2.Tag!.Name,
                TagColor    = ct2.Tag.Color,
                TagCategory = ct2.Tag.Category,
                AssignedAt  = ct2.CreatedAt
            })
            .ToListAsync(ct);
    }
}

// ── ContactProfile ────────────────────────────────────────────────────────────

public record GetContactProfileQuery(Guid ContactId) : IRequest<ContactProfileDto?>;

public sealed class GetContactProfileHandler : IRequestHandler<GetContactProfileQuery, ContactProfileDto?>
{
    private readonly IUnitOfWork _uow;
    public GetContactProfileHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<ContactProfileDto?> Handle(GetContactProfileQuery q, CancellationToken ct)
    {
        var profile = _uow.ContactProfiles.Query()
            .FirstOrDefault(p => p.ContactId == q.ContactId);

        return profile is null ? null : UpsertContactProfileHandler.MapProfile(profile);
    }
}

// ── MentorRelationships ───────────────────────────────────────────────────────

public record GetMentoringByContactQuery(Guid ContactId) : IRequest<IReadOnlyList<MentorRelationshipDto>>;

public sealed class GetMentoringByContactHandler
    : IRequestHandler<GetMentoringByContactQuery, IReadOnlyList<MentorRelationshipDto>>
{
    private readonly IUnitOfWork _uow;
    public GetMentoringByContactHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<IReadOnlyList<MentorRelationshipDto>> Handle(
        GetMentoringByContactQuery q, CancellationToken ct)
    {
        return await _uow.MentorRelationships.Query()
            .Where(r => r.MentorContactId == q.ContactId || r.MenteeContactId == q.ContactId)
            .Include(r => r.Mentor)
            .Include(r => r.Mentee)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new MentorRelationshipDto
            {
                Id               = r.Id,
                MentorContactId  = r.MentorContactId,
                MentorName       = r.Mentor!.FullName,
                MenteeContactId  = r.MenteeContactId,
                MenteeName       = r.Mentee!.FullName,
                Type             = r.Type,
                StartDate        = r.StartDate,
                EndDate          = r.EndDate,
                IsActive         = r.IsActive,
                Notes            = r.Notes,
                CreatedAt        = r.CreatedAt
            })
            .ToListAsync(ct);
    }
}

// ── Full admin view ───────────────────────────────────────────────────────────

public record GetAdminContactDetailQuery(Guid ContactId) : IRequest<AdminContactDetailDto>;

public sealed class GetAdminContactDetailHandler
    : IRequestHandler<GetAdminContactDetailQuery, AdminContactDetailDto>
{
    private readonly IUnitOfWork _uow;
    public GetAdminContactDetailHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<AdminContactDetailDto> Handle(GetAdminContactDetailQuery q, CancellationToken ct)
    {
        var contact = await _uow.Contacts.Query()
            .Include(c => c.ContactTags).ThenInclude(ct2 => ct2.Tag)
            .FirstOrDefaultAsync(c => c.Id == q.ContactId, ct)
            ?? throw new NotFoundException(nameof(Contact), q.ContactId);

        var profile = _uow.ContactProfiles.Query()
            .FirstOrDefault(p => p.ContactId == q.ContactId);

        var mentoring = await _uow.MentorRelationships.Query()
            .Where(r => r.MentorContactId == q.ContactId || r.MenteeContactId == q.ContactId)
            .Include(r => r.Mentor)
            .Include(r => r.Mentee)
            .ToListAsync(ct);

        return new AdminContactDetailDto
        {
            ContactId = contact.Id,
            FullName  = contact.FullName,
            Email     = contact.Email,
            Profile   = profile is null ? null : UpsertContactProfileHandler.MapProfile(profile),
            Tags      = contact.ContactTags.Select(ct2 => new ContactTagDto
            {
                TagId       = ct2.TagId,
                TagName     = ct2.Tag?.Name ?? string.Empty,
                TagColor    = ct2.Tag?.Color,
                TagCategory = ct2.Tag?.Category,
                AssignedAt  = ct2.CreatedAt
            }).ToList().AsReadOnly(),
            AsMentor = mentoring
                .Where(r => r.MentorContactId == q.ContactId)
                .Select(r => new MentorRelationshipDto
                {
                    Id = r.Id, MentorContactId = r.MentorContactId,
                    MentorName = r.Mentor?.FullName ?? string.Empty,
                    MenteeContactId = r.MenteeContactId,
                    MenteeName = r.Mentee?.FullName ?? string.Empty,
                    Type = r.Type, StartDate = r.StartDate, EndDate = r.EndDate,
                    IsActive = r.IsActive, Notes = r.Notes, CreatedAt = r.CreatedAt
                }).ToList().AsReadOnly(),
            AsMentee = mentoring
                .Where(r => r.MenteeContactId == q.ContactId)
                .Select(r => new MentorRelationshipDto
                {
                    Id = r.Id, MentorContactId = r.MentorContactId,
                    MentorName = r.Mentor?.FullName ?? string.Empty,
                    MenteeContactId = r.MenteeContactId,
                    MenteeName = r.Mentee?.FullName ?? string.Empty,
                    Type = r.Type, StartDate = r.StartDate, EndDate = r.EndDate,
                    IsActive = r.IsActive, Notes = r.Notes, CreatedAt = r.CreatedAt
                }).ToList().AsReadOnly()
        };
    }
}
