using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Reports.DTOs;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Reports.Queries;

public class GetReportsQuery : IRequest<PagedResult<MissionReportListDto>>
{
    public PaginationParams Params { get; }
    public Guid? CampaignId { get; }
    public Guid? ProjectId  { get; }
    public ReportStatus? Status { get; }
    public GetReportsQuery(PaginationParams p, Guid? campaignId, Guid? projectId, ReportStatus? status)
    { Params = p; CampaignId = campaignId; ProjectId = projectId; Status = status; }
}

public sealed class GetReportsHandler : IRequestHandler<GetReportsQuery, PagedResult<MissionReportListDto>>
{
    private readonly IUnitOfWork _uow;
    public GetReportsHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<PagedResult<MissionReportListDto>> Handle(GetReportsQuery q, CancellationToken ct)
    {
        var p = q.Params;
        var query = _uow.Reports.Query();
        if (q.CampaignId.HasValue) query = query.Where(r => r.CampaignId == q.CampaignId);
        if (q.ProjectId.HasValue)  query = query.Where(r => r.ProjectId  == q.ProjectId);
        if (q.Status.HasValue)     query = query.Where(r => r.Status     == q.Status);
        if (!string.IsNullOrWhiteSpace(p.Search))
        {
            var term = p.Search.ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(term) || (r.Location != null && r.Location.ToLower().Contains(term)) || r.AuthorName.ToLower().Contains(term));
        }
        query = query.OrderByDescending(r => r.ReportDate);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize)
            .Select(r => new MissionReportListDto
            {
                Id = r.Id, Title = r.Title, ReportDate = r.ReportDate, Location = r.Location,
                Status = r.Status, Language = r.Language, AuthorName = r.AuthorName,
                CampaignName = r.Campaign != null ? r.Campaign.Name : null,
                ProjectName  = r.Project  != null ? r.Project.Name  : null,
                DecisionTotal = r.Decisions.Where(d => !d.IsDeleted).Sum(d => d.Count),
                PrayerCount   = r.PrayerPoints.Count(pp => !pp.IsDeleted),
                CreatedAt = r.CreatedAt, SubmittedAt = r.SubmittedAt, ApprovedAt = r.ApprovedAt
            }).ToListAsync(ct);
        return new PagedResult<MissionReportListDto> { Items = items, Page = p.Page, PageSize = p.PageSize, TotalCount = total };
    }
}
