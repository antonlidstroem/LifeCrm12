// src/LifeCrm.Infrastructure/Services/EmailService.cs
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
        _logger = logger;
    }

    public async Task SendAsync(
        string toEmail, string toName, string subject, string htmlBody,
        IEnumerable<EmailAttachment>? attachments = null,
        CancellationToken ct = default)
    {
        var s = await _emailSettings.GetAsync(ct);

        if (s.DryRun)
        {
            _logger.LogInformation("[Email DryRun] To={To} Subject={Subject}", toEmail, subject);
            return;
        }

        if (string.IsNullOrWhiteSpace(s.Host))
            throw new InvalidOperationException(
                "Outgoing email host is not configured. " +
                "Go to Admin → E-postinställningar to configure SMTP.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(s.FromName, s.FromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        if (attachments is not null)
            foreach (var att in attachments)
                builder.Attachments.Add(att.FileName, att.Bytes, ContentType.Parse(att.ContentType));
        message.Body = builder.ToMessageBody();

        var secureSocket = s.Port switch
        {
            465 => SecureSocketOptions.SslOnConnect,
            25 or 1025 or 2525 => SecureSocketOptions.None,
            _ => s.UseSsl
                                    ? SecureSocketOptions.StartTlsWhenAvailable
                                    : SecureSocketOptions.None
        };

        using var client = new SmtpClient();

        // FIX: Disable certificate revocation checks.
        // Many corporate SMTP servers and self-signed certs fail revocation checks.
        // This is safe for internal/corporate SMTP relays. For production with public
        // CAs the revocation check passes anyway.
        client.CheckCertificateRevocation = false;

        // Also accept server certificates that the OS can't validate (self-signed).
        // Remove this line if you only use trusted CAs like Gmail/Office365/SendGrid.
        client.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;

        await client.ConnectAsync(s.Host, s.Port, secureSocket, ct);

        if (!string.IsNullOrEmpty(s.Username) && !string.IsNullOrEmpty(s.Password))
            await client.AuthenticateAsync(s.Username, s.Password, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        _logger.LogInformation("Email sent → {To} | Subject: {Subject}", toEmail, subject);
    }
}