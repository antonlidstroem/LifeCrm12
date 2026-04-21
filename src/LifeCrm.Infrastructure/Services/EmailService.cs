using LifeCrm.Core.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace LifeCrm.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IEmailSettingsService _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IEmailSettingsService settings, ILogger<EmailService> logger)
    {
        _settings = settings;
        _logger   = logger;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody,
        IEnumerable<EmailAttachment>? attachments = null, CancellationToken ct = default)
    {
        var cfg = await _settings.GetAsync(ct);

        if (cfg.DryRun)
        {
            _logger.LogInformation("[DryRun] Email to {To} — Subject: {Subject}", toEmail, subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(cfg.FromName, cfg.FromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        if (attachments is not null)
            foreach (var att in attachments)
                builder.Attachments.Add(att.FileName, att.Bytes, ContentType.Parse(att.ContentType));

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(cfg.Host, cfg.Port,
            cfg.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, ct);

        if (!string.IsNullOrEmpty(cfg.Username))
            await client.AuthenticateAsync(cfg.Username, cfg.Password, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
