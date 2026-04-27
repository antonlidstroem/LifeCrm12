using LifeCrm.Contracts.Common.DTOs;
using LifeCrm.Contracts.Donations.DTOs;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Donations.Queries;

public sealed class GetDonationsQuery : IRequest<PagedResult<DonationListDto>>
{
    public PaginationParams Params { get; }
    public Guid? ContactId  { get; }
    public DateOnly? FromDate { get; }
    public DateOnly? ToDate   { get; }
    public Guid? CampaignId { get; }
    public Guid? ProjectId  { get; }

    public GetDonationsQuery(PaginationParams p, Guid? contactId = null, DateOnly? fromDate = null,
        DateOnly? toDate = null, Guid? campaignId = null, Guid? projectId = null)
    { Params = p; ContactId = contactId; FromDate = fromDate; ToDate = toDate; CampaignId = campaignId; ProjectId = projectId; }
}

public sealed class GetDonationsHandler : IRequestHandler<GetDonationsQuery, PagedResult<DonationListDto>>
{
    private readonly IUnitOfWork _uow;
    public GetDonationsHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<PagedResult<DonationListDto>> Handle(GetDonationsQuery query, CancellationToken cancellationToken)
    {
        var p = query.Params;
        var q = _uow.Donations.Query();
        if (query.ContactId.HasValue)  q = q.Where(d => d.ContactId  == query.ContactId.Value);
        if (query.CampaignId.HasValue) q = q.Where(d => d.CampaignId == query.CampaignId.Value);
        if (query.ProjectId.HasValue)  q = q.Where(d => d.ProjectId  == query.ProjectId.Value);
        if (query.FromDate.HasValue)   q = q.Where(d => d.Date >= query.FromDate.Value);
        if (query.ToDate.HasValue)     q = q.Where(d => d.Date <= query.ToDate.Value);
        if (!string.IsNullOrWhiteSpace(p.Search)) { var term = p.Search.ToLower(); q = q.Where(d => (d.Contact != null && d.Contact.Name.ToLower().Contains(term)) || (d.ReferenceNumber != null && d.ReferenceNumber.ToLower().Contains(term))); }
        q = (p.SortBy?.ToLower()) switch { "amount" => p.SortAscending ? q.OrderBy(d => d.Amount) : q.OrderByDescending(d => d.Amount), "date" => p.SortAscending ? q.OrderBy(d => d.Date) : q.OrderByDescending(d => d.Date), _ => q.OrderByDescending(d => d.Date) };
        var totalCount = await q.CountAsync(cancellationToken);
        var items = await q.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize)
            .Select(d => new DonationListDto
            {
                Id = d.Id, ContactId = d.ContactId, ContactName = d.Contact != null ? d.Contact.Name : string.Empty,
                Amount = d.Amount, Date = d.Date, Status = d.Status, CampaignName = d.Campaign != null ? d.Campaign.Name : null,
                ProjectName = d.Project != null ? d.Project.Name : null, PaymentMethod = d.PaymentMethod,
                ReceiptSent = d.ReceiptSent, CreatedAt = d.CreatedAt,
                ReceiptDocumentId = d.Documents.Where(doc => doc.Type == DocumentType.DonationReceipt).OrderByDescending(doc => doc.CreatedAt).Select(doc => (Guid?)doc.Id).FirstOrDefault()
            }).ToListAsync(cancellationToken);
        return new PagedResult<DonationListDto> { Items = items, Page = p.Page, PageSize = p.PageSize, TotalCount = totalCount };
    }
}
