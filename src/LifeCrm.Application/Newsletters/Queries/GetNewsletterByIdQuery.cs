using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Newsletters.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Newsletters.Queries;

public class GetNewsletterByIdQuery : IRequest<NewsletterDetailDto>
{
    public Guid NewsletterId { get; }
    public GetNewsletterByIdQuery(Guid id) { NewsletterId = id; }
}

public sealed class GetNewsletterByIdHandler : IRequestHandler<GetNewsletterByIdQuery, NewsletterDetailDto>
{
    private readonly IUnitOfWork _uow;
    public GetNewsletterByIdHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<NewsletterDetailDto> Handle(GetNewsletterByIdQuery q, CancellationToken ct)
    {
        var n = await _uow.Newsletters.Query().Include(n => n.Attachments.Where(a => !a.IsDeleted)).FirstOrDefaultAsync(n => n.Id == q.NewsletterId, ct)
            ?? throw new NotFoundException(nameof(Newsletter), q.NewsletterId);
        return new NewsletterDetailDto
        {
            Id = n.Id, Title = n.Title, Subject = n.Subject, HtmlBody = n.HtmlBody, Status = n.Status,
            CreatedAt = n.CreatedAt, SentAt = n.SentAt, SentBy = n.SentBy, SentCount = n.SentCount,
            SkippedCount = n.SkippedCount, ErrorCount = n.ErrorCount, TagFilter = n.TagFilter,
            ContactTypeFilter = n.ContactTypeFilter, AttachmentCount = n.Attachments.Count,
            Attachments = n.Attachments.Select(a => new AttachmentDto { Id = a.Id, FileName = a.FileName, ContentType = a.ContentType, FileSizeBytes = a.FileSizeBytes }).ToList().AsReadOnly()
        };
    }
}
