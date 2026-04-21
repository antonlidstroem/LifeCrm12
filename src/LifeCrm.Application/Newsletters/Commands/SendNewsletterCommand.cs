using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Newsletters.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Application.Newsletters.Commands;

public class SendNewsletterCommand : IRequest<NewsletterSendResultDto>
{
    public Guid NewsletterId { get; }
    public SendNewsletterRequest Request { get; }
    public SendNewsletterCommand(Guid id, SendNewsletterRequest r) { NewsletterId = id; Request = r; }
}

public sealed class SendNewsletterHandler : IRequestHandler<SendNewsletterCommand, NewsletterSendResultDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _email;
    private readonly ICurrentUserService _cu;
    private readonly IAppSettings _appSettings;
    private readonly IUnsubscribeTokenService _tokenService;
    private readonly IConsentService _consent;
    private readonly ILogger<SendNewsletterHandler> _logger;

    public SendNewsletterHandler(
        IUnitOfWork uow,
        IEmailService email,
        ICurrentUserService cu,
        IAppSettings appSettings,
        IUnsubscribeTokenService tokenService,
        IConsentService consent,
        ILogger<SendNewsletterHandler> logger)
    {
        _uow          = uow;
        _email        = email;
        _cu           = cu;
        _appSettings  = appSettings;
        _tokenService = tokenService;
        _consent      = consent;
        _logger       = logger;
    }

    public async Task<NewsletterSendResultDto> Handle(
        SendNewsletterCommand cmd, CancellationToken ct)
    {
        var nl = await _uow.Newsletters.Query()
            .Include(n => n.Attachments.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(n => n.Id == cmd.NewsletterId, ct)
            ?? throw new NotFoundException(nameof(Newsletter), cmd.NewsletterId);

        if (nl.Status == NewsletterStatus.Sent)
            throw new ConflictException("This newsletter has already been sent.");
        if (string.IsNullOrWhiteSpace(nl.HtmlBody))
            throw new ValidationException("HtmlBody", "Cannot send a newsletter with an empty body.");

        var req         = cmd.Request;
        var attachments = nl.Attachments
            .Select(a => new EmailAttachment(a.FileBytes, a.FileName, a.ContentType))
            .ToList();

        // ── Build candidate recipient list ─────────────────────────────────────
        // The DB query filters on EmailOptOut and non-null email (fast, indexed).
        // The consent check below is the authoritative gate per recipient.
        var candidates = await PreviewNewsletterRecipientsHandler
            .BuildRecipientQuery(_uow, req.TagFilter, req.ContactTypeFilter)
            .Where(c => !c.EmailOptOut && c.Email != null && c.Email != string.Empty && !c.IsAnonymized)
            .Select(c => new { c.Id, c.FullName, c.Email })
            .ToListAsync(ct);

        int sentCount = 0, skippedCount = 0, errorCount = 0;
        var errors = new List<string>();
        var appBaseUrl = _appSettings.AppBaseUrl.TrimEnd('/');

        foreach (var r in candidates)
        {
            // ── Per-recipient consent check (authoritative gate) ───────────────
            // This catches cases where the DB EmailOptOut flag is stale relative
            // to the ConsentRecord table (e.g. bulk imports that didn't set the flag).
            var hasConsent = await _consent.HasConsentAsync(
                r.Id, ConsentType.MarketingEmail, ct);

            if (!hasConsent)
            {
                skippedCount++;
                _logger.LogDebug(
                    "Newsletter {Id}: skipped {ContactId} — no MarketingEmail consent.",
                    nl.Id, r.Id);
                continue;
            }

            try
            {
                var token    = _tokenService.GenerateToken(r.Id, nl.OrganizationId);
                var unsubUrl = $"{appBaseUrl}/api/v1/unsubscribe?token={token}";
                var footer   =
                    "<br/><hr style=\"margin:32px 0;border:none;border-top:1px solid #ddd\"/>" +
                    "<p style=\"font-size:11px;color:#999;text-align:center\">" +
                    "Du f&#229;r detta e-postmeddelande fr&#229;n en organisation du &#228;r registrerad hos.<br/>" +
                    $"<a href=\"{unsubUrl}\" style=\"color:#999\">Avregistrera dig fr&#229;n framtida utskick</a></p>";
                var body = nl.HtmlBody + footer;

                await _email.SendAsync(
                    r.Email!, r.FullName, nl.Subject, body,
                    attachments.Count > 0 ? attachments : null, ct);

                sentCount++;
            }
            catch (Exception ex)
            {
                errorCount++;
                errors.Add($"{r.FullName} <{r.Email}>: {ex.Message}");
                _logger.LogWarning(
                    "Newsletter {Id} send failed for {Email}: {Err}",
                    nl.Id, r.Email, ex.Message);
            }
        }

        nl.Status       = NewsletterStatus.Sent;
        nl.SentAt       = DateTimeOffset.UtcNow;
        nl.SentBy       = _cu.UserId?.ToString();
        nl.TagFilter    = req.TagFilter;
        nl.ContactTypeFilter = req.ContactTypeFilter;
        nl.SentCount    = sentCount;
        nl.SkippedCount = skippedCount;
        nl.ErrorCount   = errorCount;
        _uow.Newsletters.Update(nl);
        await _uow.SaveChangesAsync(ct);

        return new NewsletterSendResultDto
        {
            SentCount    = sentCount,
            SkippedCount = skippedCount,
            ErrorCount   = errorCount,
            Errors       = errors.AsReadOnly()
        };
    }
}
