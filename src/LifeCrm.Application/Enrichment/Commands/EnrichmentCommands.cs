using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Enrichment.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Enrichment.Commands;

// ── Tags ──────────────────────────────────────────────────────────────────────

public record CreateTagCommand(CreateTagRequest Request) : IRequest<TagDto>;

public sealed class CreateTagHandler : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;
    public CreateTagHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

    public async Task<TagDto> Handle(CreateTagCommand cmd, CancellationToken ct)
    {
        var orgId = _cu.OrganizationId ?? throw new ForbiddenException();
        var tag = new Tag
        {
            Id             = Guid.NewGuid(),
            OrganizationId = orgId,
            Name           = cmd.Request.Name.Trim(),
            Color          = cmd.Request.Color,
            Category       = cmd.Request.Category?.Trim()
        };
        await _uow.Tags.AddAsync(tag, ct);
        await _uow.SaveChangesAsync(ct);
        return new TagDto { Id = tag.Id, Name = tag.Name, Color = tag.Color, Category = tag.Category, ContactCount = 0 };
    }
}

public record DeleteTagCommand(Guid TagId) : IRequest<Unit>;

public sealed class DeleteTagHandler : IRequestHandler<DeleteTagCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public DeleteTagHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(DeleteTagCommand cmd, CancellationToken ct)
    {
        var tag = await _uow.Tags.GetByIdAsync(cmd.TagId, ct)
            ?? throw new NotFoundException(nameof(Tag), cmd.TagId);
        _uow.Tags.Delete(tag);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public record AssignTagCommand(Guid ContactId, Guid TagId) : IRequest<Unit>;

public sealed class AssignTagHandler : IRequestHandler<AssignTagCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;
    public AssignTagHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

    public async Task<Unit> Handle(AssignTagCommand cmd, CancellationToken ct)
    {
        var orgId = _cu.OrganizationId ?? throw new ForbiddenException();

        var alreadyAssigned = _uow.ContactTags.Query()
            .Any(ct2 => ct2.ContactId == cmd.ContactId && ct2.TagId == cmd.TagId);
        if (alreadyAssigned) return Unit.Value; // idempotent

        var contactTag = new ContactTag
        {
            Id             = Guid.NewGuid(),
            OrganizationId = orgId,
            ContactId      = cmd.ContactId,
            TagId          = cmd.TagId,
            AddedByUserId  = _cu.UserId
        };
        await _uow.ContactTags.AddAsync(contactTag, ct);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public record RemoveTagCommand(Guid ContactId, Guid TagId) : IRequest<Unit>;

public sealed class RemoveTagHandler : IRequestHandler<RemoveTagCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public RemoveTagHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(RemoveTagCommand cmd, CancellationToken ct)
    {
        var contactTag = _uow.ContactTags.Query()
            .FirstOrDefault(ct2 => ct2.ContactId == cmd.ContactId && ct2.TagId == cmd.TagId)
            ?? throw new NotFoundException("ContactTag", $"{cmd.ContactId}/{cmd.TagId}");
        _uow.ContactTags.Delete(contactTag);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

// ── ContactProfile ────────────────────────────────────────────────────────────

public record UpsertContactProfileCommand(
    Guid ContactId,
    UpsertContactProfileRequest Request) : IRequest<ContactProfileDto>;

public sealed class UpsertContactProfileHandler
    : IRequestHandler<UpsertContactProfileCommand, ContactProfileDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;
    public UpsertContactProfileHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

    public async Task<ContactProfileDto> Handle(UpsertContactProfileCommand cmd, CancellationToken ct)
    {
        var orgId = _cu.OrganizationId ?? throw new ForbiddenException();
        var r = cmd.Request;

        if (!r.DataEnrichmentConsented)
            throw new ValidationException("DataEnrichmentConsented",
                "Cannot create or update a profile without explicit data enrichment consent from the contact.");

        var profile = _uow.ContactProfiles.Query()
            .FirstOrDefault(p => p.ContactId == cmd.ContactId);

        if (profile is null)
        {
            profile = new ContactProfile
            {
                Id                      = Guid.NewGuid(),
                OrganizationId          = orgId,
                ContactId               = cmd.ContactId,
                DataEnrichmentConsented = true,
                ConsentedAt             = DateTimeOffset.UtcNow,
                ConsentSource           = r.ConsentSource
            };
            await _uow.ContactProfiles.AddAsync(profile, ct);
        }

        profile.GiftsAndTalents       = r.GiftsAndTalents?.Trim();
        profile.Interests              = r.Interests?.Trim();
        profile.InvolvementOpenTo      = r.InvolvementOpenTo?.Trim();
        profile.SpiritualNotes         = r.SpiritualNotes?.Trim();
        profile.StaffPrivateNotes      = r.StaffPrivateNotes?.Trim();
        profile.DataEnrichmentConsented = r.DataEnrichmentConsented;
        profile.LastUpdatedBy          = _cu.UserId?.ToString() ?? "system";
        profile.LastModifiedAt         = DateTimeOffset.UtcNow;

        if (profile.Id != Guid.Empty) _uow.ContactProfiles.Update(profile);
        await _uow.SaveChangesAsync(ct);

        return MapProfile(profile);
    }

    internal static ContactProfileDto MapProfile(ContactProfile p) => new()
    {
        ContactId               = p.ContactId,
        GiftsAndTalents         = p.GiftsAndTalents,
        Interests               = p.Interests,
        InvolvementOpenTo       = p.InvolvementOpenTo,
        SpiritualNotes          = p.SpiritualNotes,
        StaffPrivateNotes       = p.StaffPrivateNotes,
        DataEnrichmentConsented = p.DataEnrichmentConsented,
        ConsentedAt             = p.ConsentedAt,
        ConsentSource           = p.ConsentSource,
        LastModifiedAt          = p.LastModifiedAt,
        LastUpdatedBy           = p.LastUpdatedBy
    };
}

public record DeleteContactProfileCommand(Guid ContactId) : IRequest<Unit>;

public sealed class DeleteContactProfileHandler : IRequestHandler<DeleteContactProfileCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public DeleteContactProfileHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(DeleteContactProfileCommand cmd, CancellationToken ct)
    {
        var profile = _uow.ContactProfiles.Query()
            .FirstOrDefault(p => p.ContactId == cmd.ContactId)
            ?? throw new NotFoundException(nameof(ContactProfile), cmd.ContactId);
        _uow.ContactProfiles.Delete(profile);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

// ── MentorRelationship ────────────────────────────────────────────────────────

public record CreateMentorRelationshipCommand(CreateMentorRelationshipRequest Request) : IRequest<MentorRelationshipDto>;

public sealed class CreateMentorRelationshipHandler
    : IRequestHandler<CreateMentorRelationshipCommand, MentorRelationshipDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;
    public CreateMentorRelationshipHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

    public async Task<MentorRelationshipDto> Handle(CreateMentorRelationshipCommand cmd, CancellationToken ct)
    {
        var orgId  = _cu.OrganizationId ?? throw new ForbiddenException();
        var userId = _cu.UserId ?? throw new ForbiddenException();
        var r = cmd.Request;

        if (r.MentorContactId == r.MenteeContactId)
            throw new ValidationException("MenteeContactId", "A contact cannot mentor themselves.");

        var mentor = await _uow.Contacts.GetByIdAsync(r.MentorContactId, ct)
            ?? throw new NotFoundException(nameof(Contact), r.MentorContactId);
        var mentee = await _uow.Contacts.GetByIdAsync(r.MenteeContactId, ct)
            ?? throw new NotFoundException(nameof(Contact), r.MenteeContactId);

        var rel = new MentorRelationship
        {
            Id               = Guid.NewGuid(),
            OrganizationId   = orgId,
            MentorContactId  = r.MentorContactId,
            MenteeContactId  = r.MenteeContactId,
            Type             = r.Type,
            StartDate        = r.StartDate,
            EndDate          = r.EndDate,
            Notes            = r.Notes?.Trim(),
            IsActive         = true,
            CreatedByUserId  = userId
        };

        await _uow.MentorRelationships.AddAsync(rel, ct);
        await _uow.SaveChangesAsync(ct);

        return new MentorRelationshipDto
        {
            Id               = rel.Id,
            MentorContactId  = rel.MentorContactId,
            MentorName       = mentor.FullName,
            MenteeContactId  = rel.MenteeContactId,
            MenteeName       = mentee.FullName,
            Type             = rel.Type,
            StartDate        = rel.StartDate,
            EndDate          = rel.EndDate,
            IsActive         = rel.IsActive,
            Notes            = rel.Notes,
            CreatedAt        = rel.CreatedAt
        };
    }
}

public record DeleteMentorRelationshipCommand(Guid RelationshipId) : IRequest<Unit>;

public sealed class DeleteMentorRelationshipHandler : IRequestHandler<DeleteMentorRelationshipCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public DeleteMentorRelationshipHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(DeleteMentorRelationshipCommand cmd, CancellationToken ct)
    {
        var rel = await _uow.MentorRelationships.GetByIdAsync(cmd.RelationshipId, ct)
            ?? throw new NotFoundException(nameof(MentorRelationship), cmd.RelationshipId);
        _uow.MentorRelationships.Delete(rel);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
