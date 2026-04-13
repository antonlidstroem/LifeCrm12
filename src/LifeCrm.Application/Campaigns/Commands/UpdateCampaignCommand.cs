using LifeCrm.Application.Campaigns.DTOs;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Campaigns.Commands;

public class UpdateCampaignCommand : IRequest<Unit>
{
    public UpdateCampaignRequest Request { get; }
    public UpdateCampaignCommand(UpdateCampaignRequest r) { Request = r; }
}

public sealed class UpdateCampaignHandler : IRequestHandler<UpdateCampaignCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public UpdateCampaignHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(UpdateCampaignCommand cmd, CancellationToken ct)
    {
        var c = await _uow.Campaigns.GetByIdAsync(cmd.Request.Id, ct)
            ?? throw new NotFoundException(nameof(Campaign), cmd.Request.Id);

        // Validate new project if it changed
        if (c.ProjectId != cmd.Request.ProjectId)
        {
            _ = await _uow.Projects.GetByIdAsync(cmd.Request.ProjectId, ct)
                ?? throw new NotFoundException(nameof(Project), cmd.Request.ProjectId);
        }

        c.Name        = cmd.Request.Name.Trim();
        c.Description = cmd.Request.Description?.Trim();
        c.BudgetGoal  = cmd.Request.BudgetGoal;
        c.StartDate   = cmd.Request.StartDate;
        c.EndDate     = cmd.Request.EndDate;
        c.Status      = cmd.Request.Status;
        c.Notes       = cmd.Request.Notes?.Trim();
        c.ProjectId   = cmd.Request.ProjectId;

        _uow.Campaigns.Update(c);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
