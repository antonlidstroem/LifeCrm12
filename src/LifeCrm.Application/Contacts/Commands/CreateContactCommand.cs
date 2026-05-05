using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Contacts.DTOs;
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
    public CreateContactHandler(IUnitOfWork uow, ICurrentUserService currentUser, IMediator mediator)
    { _uow = uow; _currentUser = currentUser; _mediator = mediator; }

    public async Task<Guid> Handle(CreateContactCommand command, CancellationToken cancellationToken)
    {
        var req   = command.Request;
        var orgId = _currentUser.OrganizationId ?? throw new ForbiddenException("No organization context.");
        var contact = new Contact
        {
            Id = command.AuditEntityId, OrganizationId = orgId,
            Name = req.Name.Trim(), Type = req.Type,
            Email = req.Email?.Trim().ToLowerInvariant(), Phone = req.Phone?.Trim(),
            AddressLine1 = req.AddressLine1?.Trim(), AddressLine2 = req.AddressLine2?.Trim(),
            City = req.City?.Trim(), StateProvince = req.StateProvince?.Trim(),
            PostalCode = req.PostalCode?.Trim(), Country = req.Country?.Trim(),
            Tags = req.Tags?.Trim(), Notes = req.Notes?.Trim(),
            PrimaryContactName = req.PrimaryContactName?.Trim(), EmailOptOut = req.EmailOptOut
        };
        await _uow.Contacts.AddAsync(contact, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        await _mediator.Publish(new ContactCreatedNotification(contact.Id, contact.Name), cancellationToken);
        return contact.Id;
    }
}
