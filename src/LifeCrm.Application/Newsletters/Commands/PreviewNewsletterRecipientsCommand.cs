using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Newsletters.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Newsletters.Commands;

public class PreviewNewsletterRecipientsCommand : IRequest<NewsletterPreviewDto>
{
    public string? TagFilter { get; }
    public string? ContactTypeFilter { get; }
    public PreviewNewsletterRecipientsCommand(string? tagFilter, string? contactTypeFilter)
    { TagFilter = tagFilter; ContactTypeFilter = contactTypeFilter; }
}

public sealed class PreviewNewsletterRecipientsHandler : IRequestHandler<PreviewNewsletterRecipientsCommand, NewsletterPreviewDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;
    public PreviewNewsletterRecipientsHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

    public async Task<NewsletterPreviewDto> Handle(PreviewNewsletterRecipientsCommand cmd, CancellationToken ct)
    {
        _ = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
        var q        = BuildRecipientQuery(_uow, cmd.TagFilter, cmd.ContactTypeFilter);
        var total    = await q.CountAsync(ct);
        var optedOut = await q.CountAsync(c => c.EmailOptOut, ct);
        var noEmail  = await q.CountAsync(c => !c.EmailOptOut && (c.Email == null || c.Email == string.Empty), ct);
        return new NewsletterPreviewDto { EligibleCount = Math.Max(0, total - optedOut - noEmail), OptedOutCount = optedOut, NoEmailCount = noEmail };
    }

    internal static IQueryable<Contact> BuildRecipientQuery(IUnitOfWork uow, string? tagFilter, string? contactTypeFilter)
    {
        var q = uow.Contacts.Query();
        if (!string.IsNullOrWhiteSpace(tagFilter))
        {
            foreach (var tag in tagFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(t => t.ToLower()))
            { var captured = tag; q = q.Where(c => c.Tags != null && c.Tags.ToLower().Contains(captured)); }
        }
        if (!string.IsNullOrWhiteSpace(contactTypeFilter))
        {
            var types = contactTypeFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => Enum.TryParse<ContactType>(t, true, out var parsed) ? parsed : (ContactType?)null)
                .Where(t => t.HasValue).Select(t => t!.Value).ToList();
            if (types.Count > 0) q = q.Where(c => types.Contains(c.Type));
        }
        return q;
    }
}
