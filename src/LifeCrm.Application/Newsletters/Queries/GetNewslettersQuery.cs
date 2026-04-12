using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Newsletters.DTOs;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Newsletters.Queries;

public class GetNewslettersQuery : IRequest<PagedResult<NewsletterListDto>>
{
    public PaginationParams Params { get; }
    public NewsletterStatus? StatusFilter { get; }
    public GetNewslettersQuery(PaginationParams p, NewsletterStatus? status = null) { Params = p; StatusFilter = status; }
}

public sealed class GetNewslettersHandler : IRequestHandler<GetNewslettersQuery, PagedResult<NewsletterListDto>>
{
    private readonly IUnitOfWork _uow;
    public GetNewslettersHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<PagedResult<NewsletterListDto>> Handle(GetNewslettersQuery q, CancellationToken ct)
    {
        var p = q.Params;
        var query = _uow.Newsletters.Query();
        if (q.StatusFilter.HasValue) query = query.Where(n => n.Status == q.StatusFilter.Value);
        if (!string.IsNullOrWhiteSpace(p.Search)) { var term = p.Search.ToLower(); query = query.Where(n => n.Title.ToLower().Contains(term) || n.Subject.ToLower().Contains(term)); }
        query = query.OrderByDescending(n => n.SentAt ?? n.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((p.Page - 1) * p.PageSize).Take(p.PageSize)
            .Select(n => new NewsletterListDto { Id = n.Id, Title = n.Title, Subject = n.Subject, Status = n.Status, CreatedAt = n.CreatedAt, SentAt = n.SentAt, SentBy = n.SentBy, SentCount = n.SentCount, SkippedCount = n.SkippedCount, ErrorCount = n.ErrorCount, TagFilter = n.TagFilter, ContactTypeFilter = n.ContactTypeFilter, AttachmentCount = n.Attachments.Count(a => !a.IsDeleted) })
            .ToListAsync(ct);
        return new PagedResult<NewsletterListDto> { Items = items, Page = p.Page, PageSize = p.PageSize, TotalCount = total };
    }
}
