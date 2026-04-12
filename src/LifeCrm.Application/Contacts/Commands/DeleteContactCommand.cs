using LifeCrm.Application.Common.Behaviours;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Contacts.Commands;

public sealed class DeleteContactCommand : IRequest<Unit>, IAuditableRequest
{
    public Guid   ContactId       { get; }
    public string AuditEntityName => "Contact";
    public Guid   AuditEntityId   => ContactId;
    public string AuditAction     => "Deleted";
    public DeleteContactCommand(Guid contactId) { ContactId = contactId; }
}

public sealed class DeleteContactHandler : IRequestHandler<DeleteContactCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public DeleteContactHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(DeleteContactCommand command, CancellationToken cancellationToken)
    {
        var contact = await _uow.Contacts.GetByIdAsync(command.ContactId, cancellationToken)
            ?? throw new NotFoundException(nameof(Contact), command.ContactId);
        _uow.Contacts.Delete(contact);
        await _uow.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
