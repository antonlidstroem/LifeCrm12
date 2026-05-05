using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Reports.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Reports.Commands;

public class UpsertDecisionCountCommand : IRequest<DecisionCountDto>
{
    public Guid ReportId { get; }
    public UpsertDecisionCountRequest Request { get; }
    public UpsertDecisionCountCommand(Guid reportId, UpsertDecisionCountRequest r) { ReportId = reportId; Request = r; }
}

public sealed class UpsertDecisionCountHandler : IRequestHandler<UpsertDecisionCountCommand, DecisionCountDto>
{
    private readonly IUnitOfWork _uow;
    public UpsertDecisionCountHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<DecisionCountDto> Handle(UpsertDecisionCountCommand cmd, CancellationToken ct)
    {
        var report = await _uow.Reports.GetByIdAsync(cmd.ReportId, ct)
            ?? throw new NotFoundException(nameof(MissionReport), cmd.ReportId);
        var existing = await _uow.DecisionCounts.Query()
            .FirstOrDefaultAsync(d => d.ReportId == cmd.ReportId && d.DecisionType == cmd.Request.DecisionType, ct);
        if (existing is null)
        {
            existing = new DecisionCount { Id = Guid.NewGuid(), OrganizationId = report.OrganizationId, ReportId = cmd.ReportId, DecisionType = cmd.Request.DecisionType, Count = cmd.Request.Count };
            await _uow.DecisionCounts.AddAsync(existing, ct);
        }
        else { existing.Count = cmd.Request.Count; _uow.DecisionCounts.Update(existing); }
        await _uow.SaveChangesAsync(ct);
        return new DecisionCountDto { Id = existing.Id, DecisionType = existing.DecisionType, Count = existing.Count };
    }
}
