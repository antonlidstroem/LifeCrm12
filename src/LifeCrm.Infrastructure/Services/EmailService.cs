using LifeCrm.Core.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace LifeCrm.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    public EmailService(IConfiguration config, ILogger<EmailService> logger) { _config = config; _logger = logger; }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody,
        IEnumerable<EmailAttachment>? attachments = null, CancellationToken ct = default)
    {
        var section = _config.GetSection("Email");
        bool dryRun = bool.TryParse(section["DryRun"], out var b) && b;
        if (dryRun) { _logger.LogInformation("[Email DryRun] To={To} Subject={Subject}", toEmail, subject); return; }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(section["FromName"] ?? "LifeCrm", section["FromEmail"]));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        var builder = new BodyBuilder { HtmlBody = htmlBody };
        if (attachments is not null)
            foreach (var att in attachments)
                builder.Attachments.Add(att.FileName, att.Bytes, ContentType.Parse(att.ContentType));
        message.Body = builder.ToMessageBody();

        // FIX: Make SecureSocketOptions configurable. Default to Auto for production,
        // None for local dev ports (1025, 2525, 25).
        var port = int.Parse(section["Port"] ?? "587");
        var secureSocketStr = section["SecureSocket"];
        var secureSocket = secureSocketStr switch
        {
            "None"      => SecureSocketOptions.None,
            "StartTls"  => SecureSocketOptions.StartTls,
            "SslOnConnect" => SecureSocketOptions.SslOnConnect,
            _ => port is 1025 or 2525 or 25 ? SecureSocketOptions.None : SecureSocketOptions.Auto
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(section["Host"], port, secureSocket, ct);

        var username = section["Username"];
        var password = section["Password"];
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            await client.AuthenticateAsync(username, password, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
