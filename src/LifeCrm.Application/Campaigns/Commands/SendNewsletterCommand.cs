using LifeCrm.Application.Campaigns.DTOs;
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Application.Campaigns.Commands;

public class SendNewsletterCommand : IRequest<NewsletterResultDto>
{
    public Guid CampaignId { get; }
    public SendNewsletterRequest Request { get; }
    public SendNewsletterCommand(Guid campaignId, SendNewsletterRequest request) { CampaignId = campaignId; Request = request; }
}

public sealed class SendNewsletterHandler : IRequestHandler<SendNewsletterCommand, NewsletterResultDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _email;
    private readonly ICurrentUserService _cu;
    private readonly ILogger<SendNewsletterHandler> _logger;

    public SendNewsletterHandler(IUnitOfWork uow, IEmailService email, ICurrentUserService cu, ILogger<SendNewsletterHandler> logger)
    { _uow = uow; _email = email; _cu = cu; _logger = logger; }

    public async Task<NewsletterResultDto> Handle(SendNewsletterCommand cmd, CancellationToken ct)
    {
        _ = await _uow.Campaigns.GetByIdAsync(cmd.CampaignId, ct)
            ?? throw new NotFoundException(nameof(Core.Entities.Campaign), cmd.CampaignId);
        _ = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
        var req = cmd.Request;
        var query = _uow.Contacts.Query().Where(c => c.Email != null && c.Email != string.Empty && !c.EmailOptOut);
        if (!string.IsNullOrWhiteSpace(req.TagFilter))
        {
            var tags = req.TagFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(t => t.ToLower()).ToList();
            foreach (var tag in tags) { var cap = tag; query = query.Where(c => c.Tags != null && c.Tags.ToLower().Contains(cap)); }
        }
        var recipients = await query.Select(c => new { c.Id, c.Name, c.Email }).ToListAsync(ct);
        int sentCount = 0, errorCount = 0;
        var errors = new List<string>();
        foreach (var r in recipients)
        {
            try { await _email.SendAsync(r.Email!, r.Name, req.Subject, req.HtmlBody, ct: ct); sentCount++; }
            catch (Exception ex) { errorCount++; errors.Add($"{r.Name} <{r.Email}>: {ex.Message}"); _logger.LogWarning("Newsletter send failed for {Id}: {Err}", r.Id, ex.Message); }
        }
        return new NewsletterResultDto { SentCount = sentCount, SkippedCount = 0, ErrorCount = errorCount, Errors = errors.AsReadOnly() };
    }
}
