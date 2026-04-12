using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Reports.Commands;

public class DeleteReportCommand : IRequest<Unit>
{
    public Guid ReportId { get; }
    public DeleteReportCommand(Guid id) { ReportId = id; }
}

public sealed class DeleteReportHandler : IRequestHandler<DeleteReportCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public DeleteReportHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(DeleteReportCommand cmd, CancellationToken ct)
    {
        var rpt = await _uow.Reports.GetByIdAsync(cmd.ReportId, ct)
            ?? throw new NotFoundException(nameof(MissionReport), cmd.ReportId);
        if (rpt.Status == ReportStatus.Approved)
            throw new ConflictException("Approved reports cannot be deleted.");
        _uow.Reports.Delete(rpt);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
