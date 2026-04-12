using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Interactions.Commands;

public sealed class DeleteInteractionCommand : IRequest<Unit>
{
    public Guid InteractionId { get; }
    public DeleteInteractionCommand(Guid id) { InteractionId = id; }
}

public sealed class DeleteInteractionHandler : IRequestHandler<DeleteInteractionCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public DeleteInteractionHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(DeleteInteractionCommand cmd, CancellationToken ct)
    {
        var i = await _uow.Interactions.GetByIdAsync(cmd.InteractionId, ct) ?? throw new NotFoundException(nameof(Interaction), cmd.InteractionId);
        _uow.Interactions.Delete(i);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
