using LifeCrm.Application.Campaigns.DTOs;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Campaigns.Commands;

public class CreateCampaignCommand : IRequest<Guid>
{
    public CreateCampaignRequest Request { get; }
    public CreateCampaignCommand(CreateCampaignRequest r) { Request = r; }
}

public sealed class CreateCampaignHandler : IRequestHandler<CreateCampaignCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;
    public CreateCampaignHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

    public async Task<Guid> Handle(CreateCampaignCommand cmd, CancellationToken ct)
    {
        var orgId = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
        var c = new Campaign
        {
            Id = Guid.NewGuid(), OrganizationId = orgId,
            Name = cmd.Request.Name.Trim(), Description = cmd.Request.Description?.Trim(),
            BudgetGoal = cmd.Request.BudgetGoal, StartDate = cmd.Request.StartDate,
            EndDate = cmd.Request.EndDate, Status = cmd.Request.Status, Notes = cmd.Request.Notes?.Trim()
        };
        await _uow.Campaigns.AddAsync(c, ct);
        await _uow.SaveChangesAsync(ct);
        return c.Id;
    }
}
