using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Reports.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Reports.Commands;

public class UpdateReportCommand : IRequest<Unit>
{
    public UpdateReportRequest Request { get; }
    public UpdateReportCommand(UpdateReportRequest r) { Request = r; }
}

public sealed class UpdateReportHandler : IRequestHandler<UpdateReportCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public UpdateReportHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(UpdateReportCommand cmd, CancellationToken ct)
    {
        var r   = cmd.Request;
        var rpt = await _uow.Reports.GetByIdAsync(r.Id, ct)
            ?? throw new NotFoundException(nameof(MissionReport), r.Id);
        if (rpt.Status == ReportStatus.Approved)
            throw new ConflictException("Approved reports cannot be edited.");
        rpt.Title = r.Title.Trim(); rpt.ReportDate = r.ReportDate;
        rpt.Location = r.Location?.Trim(); rpt.CampaignId = r.CampaignId;
        rpt.ProjectId = r.ProjectId; rpt.Language = r.Language; rpt.HtmlBody = r.HtmlBody;
        rpt.EventsHeld = r.EventsHeld; rpt.TotalAttendees = r.TotalAttendees;
        rpt.NewContacts = r.NewContacts; rpt.MaterialsDistributed = r.MaterialsDistributed;
        _uow.Reports.Update(rpt);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
