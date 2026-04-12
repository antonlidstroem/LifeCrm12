using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Projects.Commands;

public class DeleteProjectCommand : IRequest<Unit>
{
    public Guid ProjectId { get; }
    public DeleteProjectCommand(Guid id) { ProjectId = id; }
}

public sealed class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public DeleteProjectHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(DeleteProjectCommand cmd, CancellationToken ct)
    {
        var p = await _uow.Projects.GetByIdAsync(cmd.ProjectId, ct) ?? throw new NotFoundException(nameof(Project), cmd.ProjectId);
        _uow.Projects.Delete(p);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
