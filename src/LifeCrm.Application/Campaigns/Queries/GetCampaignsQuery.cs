// src/LifeCrm.Application/Campaigns/Queries/GetCampaignsQuery.cs
using LifeCrm.Application.Campaigns.DTOs;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Campaigns.Queries;

public class GetCampaignsQuery : IRequest<PagedResult<CampaignListDto>>
{
    public PaginationParams Params { get; }
    public Guid? ProjectId { get; }

    public GetCampaignsQuery(PaginationParams p, Guid? projectId = null)
    {
        Params = p;
        ProjectId = projectId;
    }
}

public sealed class GetCampaignsHandler : IRequestHandler<GetCampaignsQuery, PagedResult<CampaignListDto>>
{
    private readonly IUnitOfWork _uow;
    public GetCampaignsHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<PagedResult<CampaignListDto>> Handle(GetCampaignsQuery q, CancellationToken ct)
    {
        var p = q.Params;
        var query = _uow.Campaigns.Query();

        if (q.ProjectId.HasValue)
            query = query.Where(c => c.ProjectId == q.ProjectId.Value);

        if (!string.IsNullOrWhiteSpace(p.Search))
        {
            var t = p.Search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(t));
        }

        query = p.SortAscending
            ? query.OrderBy(c => c.Name)
            : query.OrderByDescending(c => c.Name);

        var total = await query.CountAsync(ct);

        // FIX: Do NOT use .Include() with .Select() — EF translates the FK join
        // inside the projection using subqueries instead.
        var items = await query
            .Skip((p.Page - 1) * p.PageSize).Take(p.PageSize)
            .Select(c => new CampaignListDto
            {
                Id = c.Id,
                Name = c.Name,
                Status = c.Status,
                BudgetGoal = c.BudgetGoal,
                ProjectId = c.ProjectId,
                // EF translates this subquery — no .Include() needed
                ProjectName = c.Project != null ? c.Project.Name : string.Empty,
                TotalRaised = c.Donations
                    .Where(d => !d.IsDeleted)
                    .Sum(d => (decimal?)d.Amount) ?? 0,
                ProgressPercent = c.BudgetGoal.HasValue && c.BudgetGoal > 0
                    ? Math.Round(
                        (c.Donations.Where(d => !d.IsDeleted).Sum(d => (decimal?)d.Amount) ?? 0)
                        / c.BudgetGoal.Value * 100, 1)
                    : null,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                DonationCount = c.Donations.Count(d => !d.IsDeleted)
            })
            .ToListAsync(ct);

        return new PagedResult<CampaignListDto>
        {
            Items = items,
            Page = p.Page,
            PageSize = p.PageSize,
            TotalCount = total
        };
    }
}