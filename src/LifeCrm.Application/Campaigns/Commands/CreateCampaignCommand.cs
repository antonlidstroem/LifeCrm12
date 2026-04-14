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

    public CreateCampaignHandler(IUnitOfWork uow, ICurrentUserService cu)
    {
        _uow = uow;
        _cu  = cu;
    }

    public async Task<Guid> Handle(CreateCampaignCommand cmd, CancellationToken ct)
    {
        var orgId = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");

        // Validator guarantees ProjectId is non-null by this point,
        // but we guard defensively for belt-and-suspenders safety.
        if (!cmd.Request.ProjectId.HasValue || cmd.Request.ProjectId.Value == Guid.Empty)
            throw new ValidationException("ProjectId", "A campaign must be linked to a project.");

        // Verify the project exists and belongs to this org
        _ = await _uow.Projects.GetByIdAsync(cmd.Request.ProjectId.Value, ct)
            ?? throw new NotFoundException(nameof(Project), cmd.Request.ProjectId.Value);

        var c = new Campaign
        {
            Id             = Guid.NewGuid(),
            OrganizationId = orgId,
            ProjectId      = cmd.Request.ProjectId,
            Name           = cmd.Request.Name.Trim(),
            Description    = cmd.Request.Description?.Trim(),
            BudgetGoal     = cmd.Request.BudgetGoal,
            StartDate      = cmd.Request.StartDate,
            EndDate        = cmd.Request.EndDate,
            Status         = cmd.Request.Status,
            Notes          = cmd.Request.Notes?.Trim()
        };
        await _uow.Campaigns.AddAsync(c, ct);
        await _uow.SaveChangesAsync(ct);
        return c.Id;
    }
}
