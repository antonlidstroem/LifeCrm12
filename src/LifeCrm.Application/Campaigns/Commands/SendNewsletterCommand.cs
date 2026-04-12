using LifeCrm.Application.Campaigns.DTOs;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Newsletters.DTOs;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Application.Campaigns.Commands;

/// <summary>
/// Sends a simple ad-hoc newsletter scoped to a campaign (legacy feature).
/// Uses the same shared DTOs as the full Newsletter feature.
/// </summary>
public class CampaignSendNewsletterCommand : IRequest<NewsletterSendResultDto>
{
    public Guid CampaignId { get; }
    public CampaignSendNewsletterRequest Request { get; }
    public CampaignSendNewsletterCommand(Guid campaignId, CampaignSendNewsletterRequest request)
    { CampaignId = campaignId; Request = request; }
}

public sealed class CampaignSendNewsletterHandler : IRequestHandler<CampaignSendNewsletterCommand, NewsletterSendResultDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _email;
    private readonly ICurrentUserService _cu;
    private readonly ILogger<CampaignSendNewsletterHandler> _logger;

    public CampaignSendNewsletterHandler(IUnitOfWork uow, IEmailService email, ICurrentUserService cu,
        ILogger<CampaignSendNewsletterHandler> logger)
    { _uow = uow; _email = email; _cu = cu; _logger = logger; }

    public async Task<NewsletterSendResultDto> Handle(CampaignSendNewsletterCommand cmd, CancellationToken ct)
    {
        _ = await _uow.Campaigns.GetByIdAsync(cmd.CampaignId, ct)
            ?? throw new NotFoundException(nameof(Core.Entities.Campaign), cmd.CampaignId);
        _ = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");

        var req   = cmd.Request;
        var query = _uow.Contacts.Query()
            .Where(c => c.Email != null && c.Email != string.Empty && !c.EmailOptOut);

        if (!string.IsNullOrWhiteSpace(req.TagFilter))
        {
            foreach (var tag in req.TagFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                         .Select(t => t.ToLower()))
            { var cap = tag; query = query.Where(c => c.Tags != null && c.Tags.ToLower().Contains(cap)); }
        }

        var recipients = await query.Select(c => new { c.Id, c.Name, c.Email }).ToListAsync(ct);
        int sentCount = 0, errorCount = 0;
        var errors = new List<string>();

        foreach (var r in recipients)
        {
            try
            {
                await _email.SendAsync(r.Email!, r.Name, req.Subject, req.HtmlBody, ct: ct);
                sentCount++;
            }
            catch (Exception ex)
            {
                errorCount++;
                errors.Add($"{r.Name} <{r.Email}>: {ex.Message}");
                _logger.LogWarning("Campaign newsletter send failed for {Id}: {Err}", r.Id, ex.Message);
            }
        }

        return new NewsletterSendResultDto
        {
            SentCount    = sentCount,
            SkippedCount = 0,
            ErrorCount   = errorCount,
            Errors       = errors.AsReadOnly()
        };
    }
}
