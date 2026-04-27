using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Documents.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Documents.Commands;

public class GenerateDonationReceiptCommand : IRequest<DocumentDto>
{
    public GenerateDonationReceiptRequest Request { get; }
    public GenerateDonationReceiptCommand(GenerateDonationReceiptRequest r) { Request = r; }
}

public sealed class GenerateDonationReceiptHandler : IRequestHandler<GenerateDonationReceiptCommand, DocumentDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPdfService _pdf;
    private readonly IEmailService _email;
    private readonly ICurrentUserService _cu;
    private readonly IOrganizationReader _orgReader;

    public GenerateDonationReceiptHandler(IUnitOfWork uow, IPdfService pdf, IEmailService email, ICurrentUserService cu, IOrganizationReader orgReader)
    { _uow = uow; _pdf = pdf; _email = email; _cu = cu; _orgReader = orgReader; }

    public async Task<DocumentDto> Handle(GenerateDonationReceiptCommand cmd, CancellationToken ct)
    {
        var req      = cmd.Request;
        var donation = await _uow.Donations.GetByIdAsync(req.DonationId, ct) ?? throw new NotFoundException(nameof(Donation), req.DonationId);
        var contact  = await _uow.Contacts.GetByIdAsync(donation.ContactId, ct) ?? throw new NotFoundException(nameof(Contact), donation.ContactId);
        var orgId    = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
        var org      = await _orgReader.GetByIdAsync(orgId, ct) ?? throw new NotFoundException(nameof(Organization), orgId);
        var receiptNumber = $"RCP-{DateTime.UtcNow:yyyyMMdd}-{donation.Id.ToString()[..8].ToUpper()}";
        var pdfBytes      = await _pdf.GenerateDonationReceiptAsync(donation, org, receiptNumber);
        var safe          = Sanitize(contact.Name);
        var fileName      = $"Receipt-{receiptNumber}-{safe}.pdf";
        var doc = new Document { Id = Guid.NewGuid(), OrganizationId = orgId, Type = DocumentType.DonationReceipt, FileName = fileName, PdfBytes = pdfBytes, ContactId = contact.Id, DonationId = donation.Id, ReceiptNumber = receiptNumber };
        await _orgReader.SaveDocumentAsync(doc, ct);
        donation.ReceiptSent = true; donation.ReceiptSentAt = DateTimeOffset.UtcNow;
        _uow.Donations.Update(donation);
        await _uow.SaveChangesAsync(ct);
        if (req.SendByEmail && !string.IsNullOrWhiteSpace(contact.Email) && !contact.EmailOptOut)
        {
            try { await _email.SendAsync(contact.Email, contact.Name, $"Donation Receipt from {org.Name}", $"<p>Dear {contact.Name},</p><p>Thank you for your donation of ${donation.Amount:N2}.</p>", new[] { new EmailAttachment(pdfBytes, fileName, "application/pdf") }, ct); }
            catch { /* email failure must not invalidate the receipt */ }
        }
        return new DocumentDto { Id = doc.Id, Type = doc.Type, FileName = doc.FileName, ReceiptNumber = receiptNumber, ContactId = contact.Id, ContactName = contact.Name, DonationId = donation.Id, CreatedAt = doc.CreatedAt };
    }

    private static string Sanitize(string name)
    {
        var s = new string(name.Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray()).Replace(" ", "-").Trim('-');
        return s.Length > 30 ? s[..30] : s;
    }
}
