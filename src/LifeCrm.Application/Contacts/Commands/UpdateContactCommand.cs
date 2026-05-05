using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Contacts.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Contacts.Commands;

public sealed class UpdateContactCommand : IRequest<Unit>, IAuditableRequest
{
    public UpdateContactRequest Request { get; }
    public string AuditEntityName => "Contact";
    public Guid   AuditEntityId   => Request.Id;
    public string AuditAction     => "Updated";
    public UpdateContactCommand(UpdateContactRequest request) { Request = request; }
}

public sealed class UpdateContactHandler : IRequestHandler<UpdateContactCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public UpdateContactHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(UpdateContactCommand command, CancellationToken cancellationToken)
    {
        var req     = command.Request;
        var contact = await _uow.Contacts.GetByIdAsync(req.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), req.Id);
        contact.Name = req.Name.Trim(); contact.Type = req.Type;
        contact.Email = req.Email?.Trim().ToLowerInvariant(); contact.Phone = req.Phone?.Trim();
        contact.AddressLine1 = req.AddressLine1?.Trim(); contact.AddressLine2 = req.AddressLine2?.Trim();
        contact.City = req.City?.Trim(); contact.StateProvince = req.StateProvince?.Trim();
        contact.PostalCode = req.PostalCode?.Trim(); contact.Country = req.Country?.Trim();
        contact.Tags = req.Tags?.Trim(); contact.Notes = req.Notes?.Trim();
        contact.PrimaryContactName = req.PrimaryContactName?.Trim(); contact.EmailOptOut = req.EmailOptOut;
        _uow.Contacts.Update(contact);
        await _uow.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
