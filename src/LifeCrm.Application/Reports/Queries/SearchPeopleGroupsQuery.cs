using LifeCrm.Contracts.Reports.DTOs;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Reports.Queries;

public class SearchPeopleGroupsQuery : IRequest<IReadOnlyList<PeopleGroupSearchDto>>
{
    public string SearchTerm { get; }
    public SearchPeopleGroupsQuery(string term) { SearchTerm = term; }
}

public sealed class SearchPeopleGroupsHandler : IRequestHandler<SearchPeopleGroupsQuery, IReadOnlyList<PeopleGroupSearchDto>>
{
    private readonly IUnitOfWork _uow;
    public SearchPeopleGroupsHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<IReadOnlyList<PeopleGroupSearchDto>> Handle(SearchPeopleGroupsQuery q, CancellationToken ct)
    {
        var term = q.SearchTerm.ToLower().Trim();
        if (string.IsNullOrWhiteSpace(term)) return Array.Empty<PeopleGroupSearchDto>();
        return await _uow.PeopleGroupSeeds.Query()
            .Where(p => p.Name.ToLower().Contains(term) || p.Country.ToLower().Contains(term) || (p.Language != null && p.Language.ToLower().Contains(term)))
            .OrderByDescending(p => p.IsUnreached).ThenBy(p => p.Name).Take(20)
            .Select(p => new PeopleGroupSearchDto { JpCode = p.JpCode, Name = p.Name, Country = p.Country, Language = p.Language, Population = p.Population, IsUnreached = p.IsUnreached })
            .ToListAsync(ct);
    }
}
