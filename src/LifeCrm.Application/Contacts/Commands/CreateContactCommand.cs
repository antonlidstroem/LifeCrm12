using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Contacts.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Contacts.Commands;

public sealed class CreateContactCommand : IRequest<Guid>, IAuditableRequest
{
    private readonly Guid _entityId = Guid.NewGuid();
    public CreateContactRequest Request { get; }
    public string AuditEntityName => "Contact";
    public Guid   AuditEntityId   => _entityId;
    public string AuditAction     => "Created";

    public CreateContactCommand(CreateContactRequest request) { Request = request; }
}

public sealed class CreateContactHandler : IRequestHandler<CreateContactCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IMediator _mediator;
    private readonly IEmailHashService _emailHash;

    public CreateContactHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        IMediator mediator,
        IEmailHashService emailHash)
    {
        _uow         = uow;
        _currentUser = currentUser;
        _mediator    = mediator;
        _emailHash   = emailHash;
    }

    public async Task<Guid> Handle(CreateContactCommand command, CancellationToken ct)
    {
        var req   = command.Request;
        var orgId = _currentUser.OrganizationId
            ?? throw new ForbiddenException("No organization context.");

        var normalizedEmail = req.Email?.Trim().ToLowerInvariant();

        var contact = new Contact
        {
            Id             = command.AuditEntityId,
            OrganizationId = orgId,

            // ── Name split ────────────────────────────────────────────────────
            FirstName      = req.FirstName.Trim(),
            LastName       = req.LastName?.Trim() ?? string.Empty,
#pragma warning disable CS0618
            Name           = $"{req.FirstName.Trim()} {req.LastName?.Trim()}".Trim(),
#pragma warning restore CS0618

            Type           = req.Type,
            Email          = normalizedEmail,
            EmailHash      = normalizedEmail != null ? _emailHash.Hash(normalizedEmail) : null,
            Phone          = req.Phone?.Trim(),
            AddressLine1   = req.AddressLine1?.Trim(),
            AddressLine2   = req.AddressLine2?.Trim(),
            City           = req.City?.Trim(),
            StateProvince  = req.StateProvince?.Trim(),
            PostalCode     = req.PostalCode?.Trim(),
            Country        = req.Country?.Trim(),
            Tags           = req.Tags?.Trim(),
            Notes          = req.Notes?.Trim(),
            PrimaryContactName = req.PrimaryContactName?.Trim(),
            EmailOptOut    = req.EmailOptOut,

            // ── Audit: explicitly capture who created this record ─────────────
            CreatedBy      = _currentUser.UserId?.ToString() ?? "system"
        };

        await _uow.Contacts.AddAsync(contact, ct);
        await _uow.SaveChangesAsync(ct);

        await _mediator.Publish(
            new ContactCreatedNotification(contact.Id, contact.FullName), ct);

        return contact.Id;
    }
}
