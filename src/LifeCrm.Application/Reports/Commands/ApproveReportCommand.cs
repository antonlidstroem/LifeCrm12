using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Reports.Commands;

public class ApproveReportCommand : IRequest<Unit>
{
    public Guid   ReportId     { get; }
    public string ApproverName { get; }
    public ApproveReportCommand(Guid id, string approverName) { ReportId = id; ApproverName = approverName; }
}

public sealed class ApproveReportHandler : IRequestHandler<ApproveReportCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    private readonly IMediator   _mediator;
    public ApproveReportHandler(IUnitOfWork uow, IMediator mediator) { _uow = uow; _mediator = mediator; }

    public async Task<Unit> Handle(ApproveReportCommand cmd, CancellationToken ct)
    {
        var rpt = await _uow.Reports.GetByIdAsync(cmd.ReportId, ct)
            ?? throw new NotFoundException(nameof(MissionReport), cmd.ReportId);
        if (rpt.Status != ReportStatus.Submitted)
            throw new ConflictException("Only Submitted reports can be approved.");
        rpt.Status = ReportStatus.Approved; rpt.ApprovedAt = DateTimeOffset.UtcNow; rpt.ApprovedBy = cmd.ApproverName;
        _uow.Reports.Update(rpt);
        await _uow.SaveChangesAsync(ct);
        await _mediator.Publish(new ReportApprovedNotification(rpt.Id, rpt.AuthorUserId, rpt.Title), ct);
        return Unit.Value;
    }
}
