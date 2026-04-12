using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Reports.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Reports.Commands;

public class ReturnReportCommand : IRequest<Unit>
{
    public Guid ReportId { get; }
    public ReturnForRevisionRequest Request { get; }
    public ReturnReportCommand(Guid id, ReturnForRevisionRequest r) { ReportId = id; Request = r; }
}

public sealed class ReturnReportHandler : IRequestHandler<ReturnReportCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public ReturnReportHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(ReturnReportCommand cmd, CancellationToken ct)
    {
        var rpt = await _uow.Reports.GetByIdAsync(cmd.ReportId, ct)
            ?? throw new NotFoundException(nameof(MissionReport), cmd.ReportId);
        if (rpt.Status != ReportStatus.Submitted)
            throw new ConflictException("Only Submitted reports can be returned.");
        rpt.Status = ReportStatus.ReturnedForRevision; rpt.ReviewComment = cmd.Request.Comment;
        _uow.Reports.Update(rpt);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
