using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Contacts.DTOs;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Contacts.Queries;

public class GetContactsQuery : IRequest<PagedResult<ContactListDto>>
{
    public PaginationParams Params { get; }
    public GetContactsQuery(PaginationParams p) { Params = p; }
}

public sealed class GetContactsHandler : IRequestHandler<GetContactsQuery, PagedResult<ContactListDto>>
{
    private readonly IUnitOfWork _uow;
    public GetContactsHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<PagedResult<ContactListDto>> Handle(GetContactsQuery query, CancellationToken ct)
    {
        var p = query.Params;
        var q = _uow.Contacts.Query();
        if (!string.IsNullOrWhiteSpace(p.Search))
        {
            var t = p.Search.ToLower();
            q = q.Where(c => c.Name.ToLower().Contains(t) || (c.Email != null && c.Email.Contains(t)) || (c.Tags != null && c.Tags.ToLower().Contains(t)));
        }
        q = (p.SortBy?.ToLower()) switch
        {
            "name"      => p.SortAscending ? q.OrderBy(c => c.Name)      : q.OrderByDescending(c => c.Name),
            "createdat" => p.SortAscending ? q.OrderBy(c => c.CreatedAt) : q.OrderByDescending(c => c.CreatedAt),
            _           => q.OrderBy(c => c.Name)
        };
        var total = await q.CountAsync(ct);
        var items = await q.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize)
            .Select(c => new ContactListDto
            {
                Id = c.Id, Name = c.Name, Type = c.Type, Email = c.Email, Phone = c.Phone, Tags = c.Tags,
                TotalDonations = c.Donations.Where(d => !d.IsDeleted).Sum(d => (decimal?)d.Amount) ?? 0,
                LastDonationDate = c.Donations.Where(d => !d.IsDeleted).OrderByDescending(d => d.Date).Select(d => (DateOnly?)d.Date).FirstOrDefault(),
                CreatedAt = c.CreatedAt
            }).ToListAsync(ct);
        return new PagedResult<ContactListDto> { Items = items, Page = p.Page, PageSize = p.PageSize, TotalCount = total };
    }
}
