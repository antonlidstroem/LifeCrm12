using LifeCrm.Application.Campaigns.DTOs;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Campaigns.Queries;

public class GetCampaignByIdQuery : IRequest<CampaignDto>
{
    public Guid CampaignId { get; }
    public GetCampaignByIdQuery(Guid id) { CampaignId = id; }
}

public sealed class GetCampaignByIdHandler : IRequestHandler<GetCampaignByIdQuery, CampaignDto>
{
    private readonly IUnitOfWork _uow;
    public GetCampaignByIdHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<CampaignDto> Handle(GetCampaignByIdQuery q, CancellationToken ct)
    {
        // Include Project navigation so we can return ProjectName
        var c = await _uow.Campaigns.Query()
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == q.CampaignId, ct)
            ?? throw new NotFoundException(nameof(Campaign), q.CampaignId);

        var raised = await _uow.Donations.Query()
            .Where(d => d.CampaignId == c.Id).SumAsync(d => (decimal?)d.Amount, ct) ?? 0;
        var count  = await _uow.Donations.CountAsync(d => d.CampaignId == c.Id, ct);

        return new CampaignDto
        {
            Id              = c.Id,
            Name            = c.Name,
            Description     = c.Description,
            Status          = c.Status,
            BudgetGoal      = c.BudgetGoal,
            TotalRaised     = raised,
            ProgressPercent = c.BudgetGoal.HasValue && c.BudgetGoal > 0
                ? Math.Round(raised / c.BudgetGoal.Value * 100, 1) : null,
            StartDate       = c.StartDate,
            EndDate         = c.EndDate,
            Notes           = c.Notes,
            ProjectId       = c.ProjectId,
            ProjectName     = c.Project?.Name ?? string.Empty,
            DonationCount   = count,
            CreatedAt       = c.CreatedAt,
            LastModifiedAt  = c.LastModifiedAt
        };
    }
}
