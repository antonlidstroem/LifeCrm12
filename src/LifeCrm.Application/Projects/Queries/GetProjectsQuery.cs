using LifeCrm.Contracts.Common.DTOs;
using LifeCrm.Contracts.Projects.DTOs;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Projects.Queries;

public class GetProjectsQuery : IRequest<PagedResult<ProjectListDto>>
{
    public PaginationParams Params { get; }
    public GetProjectsQuery(PaginationParams p) { Params = p; }
}

public sealed class GetProjectsHandler : IRequestHandler<GetProjectsQuery, PagedResult<ProjectListDto>>
{
    private readonly IUnitOfWork _uow;
    public GetProjectsHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<PagedResult<ProjectListDto>> Handle(GetProjectsQuery q, CancellationToken ct)
    {
        var p = q.Params;
        var query = _uow.Projects.Query();
        if (!string.IsNullOrWhiteSpace(p.Search)) { var t = p.Search.ToLower(); query = query.Where(x => x.Name.ToLower().Contains(t) || (x.Location != null && x.Location.ToLower().Contains(t))); }
        query = p.SortAscending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize)
            .Select(x => new ProjectListDto { Id = x.Id, Name = x.Name, Status = x.Status, Location = x.Location, BudgetGoal = x.BudgetGoal, TotalFunded = x.Donations.Where(d => !d.IsDeleted).Sum(d => (decimal?)d.Amount) ?? 0, StartDate = x.StartDate, EndDate = x.EndDate }).ToListAsync(ct);
        return new PagedResult<ProjectListDto> { Items = items, Page = p.Page, PageSize = p.PageSize, TotalCount = total };
    }
}
