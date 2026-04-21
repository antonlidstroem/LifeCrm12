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
            q = q.Where(c =>
                c.FirstName.ToLower().Contains(t) ||
                c.LastName.ToLower().Contains(t) ||
                (c.Tags != null && c.Tags.ToLower().Contains(t)));
            // Note: Email is now encrypted — searching by email hash is done via
            // a dedicated endpoint. General search uses name and tags only.
        }

        q = (p.SortBy?.ToLower()) switch
        {
            "name"      => p.SortAscending ? q.OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
                                           : q.OrderByDescending(c => c.LastName).ThenByDescending(c => c.FirstName),
            "createdat" => p.SortAscending ? q.OrderBy(c => c.CreatedAt)
                                           : q.OrderByDescending(c => c.CreatedAt),
            _           => q.OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
        };

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .Select(c => new ContactListDto
            {
                Id             = c.Id,
                FirstName      = c.FirstName,
                LastName       = c.LastName,
                Type           = c.Type,
                Email          = c.Email,   // decrypted by EF value converter
                Phone          = c.Phone,
                Tags           = c.Tags,
                IsAnonymized   = c.IsAnonymized,
                TotalDonations = c.Donations.Where(d => !d.IsDeleted).Sum(d => (decimal?)d.Amount) ?? 0,
                LastDonationDate = c.Donations.Where(d => !d.IsDeleted)
                    .OrderByDescending(d => d.Date)
                    .Select(d => (DateOnly?)d.Date)
                    .FirstOrDefault(),
                CreatedAt      = c.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<ContactListDto>
        {
            Items      = items,
            Page       = p.Page,
            PageSize   = p.PageSize,
            TotalCount = total
        };
    }
}
