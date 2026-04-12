using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Newsletters.DTOs;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Campaigns.Commands;

public class PreviewNewsletterCommand : IRequest<NewsletterPreviewDto>
{
    public Guid CampaignId { get; }
    public string? TagFilter { get; }
    public PreviewNewsletterCommand(Guid campaignId, string? tagFilter)
    { CampaignId = campaignId; TagFilter = tagFilter; }
}

public sealed class PreviewNewsletterHandler : IRequestHandler<PreviewNewsletterCommand, NewsletterPreviewDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;
    public PreviewNewsletterHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

    public async Task<NewsletterPreviewDto> Handle(PreviewNewsletterCommand cmd, CancellationToken ct)
    {
        _ = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
        var q = _uow.Contacts.Query();
        if (!string.IsNullOrWhiteSpace(cmd.TagFilter))
        {
            foreach (var tag in cmd.TagFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                         .Select(t => t.ToLower()))
            { var cap = tag; q = q.Where(c => c.Tags != null && c.Tags.ToLower().Contains(cap)); }
        }
        var total    = await q.CountAsync(ct);
        var optedOut = await q.CountAsync(c => c.EmailOptOut, ct);
        var noEmail  = await q.CountAsync(c => !c.EmailOptOut && (c.Email == null || c.Email == string.Empty), ct);
        return new NewsletterPreviewDto
        {
            EligibleCount = Math.Max(0, total - optedOut - noEmail),
            OptedOutCount = optedOut,
            NoEmailCount  = noEmail
        };
    }
}
