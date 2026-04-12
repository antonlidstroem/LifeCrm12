using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Newsletters.Commands;

public class DeleteNewsletterCommand : IRequest<Unit>
{
    public Guid NewsletterId { get; }
    public DeleteNewsletterCommand(Guid id) { NewsletterId = id; }
}

public sealed class DeleteNewsletterHandler : IRequestHandler<DeleteNewsletterCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public DeleteNewsletterHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(DeleteNewsletterCommand cmd, CancellationToken ct)
    {
        var nl = await _uow.Newsletters.GetByIdAsync(cmd.NewsletterId, ct) ?? throw new NotFoundException(nameof(Newsletter), cmd.NewsletterId);
        if (nl.Status == NewsletterStatus.Sent) throw new ConflictException("Cannot delete a newsletter that has already been sent.");
        _uow.Newsletters.Delete(nl);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
