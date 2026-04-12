using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Documents.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Documents.Commands;

public class GenerateDonationSummaryCommand : IRequest<DocumentDto>
{
    public GenerateDonationSummaryRequest Request { get; }
    public GenerateDonationSummaryCommand(GenerateDonationSummaryRequest r) { Request = r; }
}

public sealed class GenerateDonationSummaryHandler : IRequestHandler<GenerateDonationSummaryCommand, DocumentDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPdfService _pdf;
    private readonly IEmailService _email;
    private readonly ICurrentUserService _cu;
    private readonly IOrganizationReader _orgReader;

    public GenerateDonationSummaryHandler(IUnitOfWork uow, IPdfService pdf, IEmailService email, ICurrentUserService cu, IOrganizationReader orgReader)
    { _uow = uow; _pdf = pdf; _email = email; _cu = cu; _orgReader = orgReader; }

    public async Task<DocumentDto> Handle(GenerateDonationSummaryCommand cmd, CancellationToken ct)
    {
        var req     = cmd.Request;
        var contact = await _uow.Contacts.GetByIdAsync(req.ContactId, ct) ?? throw new NotFoundException(nameof(Contact), req.ContactId);
        var orgId   = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
        var org     = await _orgReader.GetByIdAsync(orgId, ct) ?? throw new NotFoundException(nameof(Organization), orgId);
        var list    = await _uow.Donations.Query().Where(d => d.ContactId == req.ContactId && d.Date >= req.PeriodStart && d.Date <= req.PeriodEnd).OrderBy(d => d.Date).ToListAsync(ct);
        var bytes   = await _pdf.GenerateDonationSummaryAsync(contact, list, org, req.PeriodStart, req.PeriodEnd);
        var safe    = new string(contact.Name.Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray()).Replace(" ", "-").Trim('-');
        if (safe.Length > 30) safe = safe[..30];
        var fn  = $"Summary-{safe}-{req.PeriodStart:yyyy}-{req.PeriodEnd:yyyy}.pdf";
        var doc = new Document { Id = Guid.NewGuid(), OrganizationId = orgId, Type = DocumentType.DonationSummary, FileName = fn, PdfBytes = bytes, ContactId = contact.Id, PeriodStart = req.PeriodStart, PeriodEnd = req.PeriodEnd };
        await _orgReader.SaveDocumentAsync(doc, ct);
        if (req.SendByEmail && !string.IsNullOrWhiteSpace(contact.Email) && !contact.EmailOptOut)
        {
            try { await _email.SendAsync(contact.Email, contact.Name, $"Donation Summary from {org.Name}", "<p>Please find your donation summary attached.</p>", new[] { new EmailAttachment(bytes, fn, "application/pdf") }, ct); }
            catch { }
        }
        return new DocumentDto { Id = doc.Id, Type = doc.Type, FileName = doc.FileName, ContactId = contact.Id, ContactName = contact.Name, PeriodStart = req.PeriodStart, PeriodEnd = req.PeriodEnd, CreatedAt = doc.CreatedAt };
    }
}
