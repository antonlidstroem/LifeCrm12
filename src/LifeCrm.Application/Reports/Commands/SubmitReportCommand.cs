using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Reports.Commands;

public class SubmitReportCommand : IRequest<Unit>
{
    public Guid ReportId { get; }
    public SubmitReportCommand(Guid id) { ReportId = id; }
}

public sealed class SubmitReportHandler : IRequestHandler<SubmitReportCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    private readonly IMediator   _mediator;
    public SubmitReportHandler(IUnitOfWork uow, IMediator mediator) { _uow = uow; _mediator = mediator; }

    public async Task<Unit> Handle(SubmitReportCommand cmd, CancellationToken ct)
    {
        var rpt = await _uow.Reports.GetByIdAsync(cmd.ReportId, ct)
            ?? throw new NotFoundException(nameof(MissionReport), cmd.ReportId);
        if (rpt.Status != ReportStatus.Draft && rpt.Status != ReportStatus.ReturnedForRevision)
            throw new ConflictException("Only Draft or Returned reports can be submitted.");
        rpt.Status = ReportStatus.Submitted; rpt.SubmittedAt = DateTimeOffset.UtcNow; rpt.ReviewComment = null;
        _uow.Reports.Update(rpt);
        await _uow.SaveChangesAsync(ct);
        await _mediator.Publish(new ReportSubmittedNotification(rpt.Id, rpt.Title, rpt.OrganizationId), ct);
        return Unit.Value;
    }
}
