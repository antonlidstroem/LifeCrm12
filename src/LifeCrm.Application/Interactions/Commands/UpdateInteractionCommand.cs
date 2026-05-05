using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Interactions.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Interactions.Commands;

public sealed class UpdateInteractionCommand : IRequest<Unit>
{
    public UpdateInteractionRequest Request { get; }
    public UpdateInteractionCommand(UpdateInteractionRequest r) { Request = r; }
}

public sealed class UpdateInteractionHandler : IRequestHandler<UpdateInteractionCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public UpdateInteractionHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(UpdateInteractionCommand cmd, CancellationToken ct)
    {
        var req = cmd.Request;
        var i = await _uow.Interactions.GetByIdAsync(req.Id, ct) ?? throw new NotFoundException(nameof(Interaction), req.Id);
        if (!req.ContactId.HasValue && !req.ProjectId.HasValue)
            throw new ValidationException("ContactId", "An interaction must be linked to a contact or a project.");
        i.Type = req.Type; i.Body = req.Body.Trim(); i.Subject = req.Subject?.Trim();
        i.OccurredAt = req.OccurredAt; i.ContactId = req.ContactId; i.ProjectId = req.ProjectId;
        i.DueDate = req.DueDate; i.IsCompleted = req.IsCompleted;
        _uow.Interactions.Update(i);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
