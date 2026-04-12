using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Reports.Commands;

public class RemovePeopleGroupCommand : IRequest<Unit>
{
    public Guid ReportId { get; }
    public Guid EntryId  { get; }
    public RemovePeopleGroupCommand(Guid reportId, Guid entryId) { ReportId = reportId; EntryId = entryId; }
}

public sealed class RemovePeopleGroupHandler : IRequestHandler<RemovePeopleGroupCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public RemovePeopleGroupHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(RemovePeopleGroupCommand cmd, CancellationToken ct)
    {
        var pg = await _uow.PeopleGroupsReached.GetByIdAsync(cmd.EntryId, ct)
            ?? throw new NotFoundException(nameof(PeopleGroupReached), cmd.EntryId);
        _uow.PeopleGroupsReached.Delete(pg);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
