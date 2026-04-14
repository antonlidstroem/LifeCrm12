using LifeCrm.Core.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace LifeCrm.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IEmailSettingsService _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IEmailSettingsService emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings;
        _logger        = logger;
    }

    public async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        IEnumerable<EmailAttachment>? attachments = null,
        CancellationToken ct = default)
    {
        var s = await _emailSettings.GetAsync(ct);

        if (s.DryRun)
        {
            _logger.LogInformation(
                "[Email DryRun] To={To} Subject={Subject} Body={BodyLen}chars",
                toEmail, subject, htmlBody?.Length ?? 0);
            return;
        }

        if (string.IsNullOrWhiteSpace(s.Host))
            throw new InvalidOperationException(
                "Outgoing email host is not configured. " +
                "Please configure SMTP settings in Admin → E-postinställningar.");

        if (string.IsNullOrWhiteSpace(s.FromEmail))
            throw new InvalidOperationException(
                "Sender email address is not configured. " +
                "Please configure SMTP settings in Admin → E-postinställningar.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(s.FromName, s.FromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        if (attachments is not null)
        {
            foreach (var att in attachments)
                builder.Attachments.Add(att.FileName, att.Bytes, ContentType.Parse(att.ContentType));
        }
        message.Body = builder.ToMessageBody();

        // Auto-detect SecureSocketOptions from port number and UseSsl flag
        var secureSocket = s.Port switch
        {
            465                => SecureSocketOptions.SslOnConnect,
            25 or 1025 or 2525 => SecureSocketOptions.None,
            _                  => s.UseSsl
                                    ? SecureSocketOptions.StartTlsWhenAvailable
                                    : SecureSocketOptions.None
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(s.Host, s.Port, secureSocket, ct);

        if (!string.IsNullOrEmpty(s.Username) && !string.IsNullOrEmpty(s.Password))
            await client.AuthenticateAsync(s.Username, s.Password, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        _logger.LogInformation("Email sent → {To} | Subject: {Subject}", toEmail, subject);
    }
}
