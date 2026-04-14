// src/LifeCrm.Application/Campaigns/Queries/GetCampaignByIdQuery.cs
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
        // Use Select projection with subquery for Project name — avoids Include+Select conflict
        var c = await _uow.Campaigns.Query()
            .Where(x => x.Id == q.CampaignId)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                x.Status,
                x.BudgetGoal,
                x.StartDate,
                x.EndDate,
                x.Notes,
                x.ProjectId,
                x.CreatedAt,
                x.LastModifiedAt,
                ProjectName = x.Project != null ? x.Project.Name : string.Empty,
                TotalRaised = x.Donations.Where(d => !d.IsDeleted).Sum(d => (decimal?)d.Amount) ?? 0,
                DonationCount = x.Donations.Count(d => !d.IsDeleted)
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(nameof(Campaign), q.CampaignId);

        return new CampaignDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Status = c.Status,
            BudgetGoal = c.BudgetGoal,
            TotalRaised = c.TotalRaised,
            ProgressPercent = c.BudgetGoal.HasValue && c.BudgetGoal > 0
                ? Math.Round(c.TotalRaised / c.BudgetGoal.Value * 100, 1) : null,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            Notes = c.Notes,
            ProjectId = c.ProjectId,
            ProjectName = c.ProjectName,
            DonationCount = c.DonationCount,
            CreatedAt = c.CreatedAt,
            LastModifiedAt = c.LastModifiedAt
        };
    }
}