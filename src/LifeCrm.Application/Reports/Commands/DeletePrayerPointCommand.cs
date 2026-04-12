using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Reports.Commands;

public class DeletePrayerPointCommand : IRequest<Unit>
{
    public Guid Id { get; }
    public DeletePrayerPointCommand(Guid id) { Id = id; }
}

public sealed class DeletePrayerPointHandler : IRequestHandler<DeletePrayerPointCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public DeletePrayerPointHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(DeletePrayerPointCommand cmd, CancellationToken ct)
    {
        var pp = await _uow.PrayerPoints.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException(nameof(PrayerPoint), cmd.Id);
        _uow.PrayerPoints.Delete(pp);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
