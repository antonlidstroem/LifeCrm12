namespace LifeCrm.Core.Interfaces;

public record EmailAttachment(byte[] Bytes, string FileName, string ContentType);

public interface IEmailService
{
    Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        IEnumerable<EmailAttachment>? attachments = null,
        CancellationToken ct = default);
}
