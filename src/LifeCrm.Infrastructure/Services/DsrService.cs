using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Infrastructure.Services;

/// <summary>
/// Data Subject Rights service.
/// Export: full JSON snapshot of all personal data held about a contact.
/// Anonymize: replaces PII with deterministic placeholders, preserves FK integrity.
/// HardDelete: removes the contact row and soft-deletes related records.
///             Only legal when no financial records exist or they have been archived.
/// </summary>
public class DsrService : IDsrService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _cu;
    private readonly IConsentService _consent;
    private readonly ILogger<DsrService> _logger;

    public DsrService(
        AppDbContext db,
        ICurrentUserService cu,
        IConsentService consent,
        ILogger<DsrService> logger)
    {
        _db = db;
        _cu = cu;
        _consent = consent;
        _logger = logger;
    }

    public async Task<ContactDataExport> ExportAsync(Guid contactId, CancellationToken ct = default)
    {
        var contact = await _db.Contacts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == contactId, ct)
            ?? throw new Application.Common.Exceptions.NotFoundException(nameof(Contact), contactId);

        var donations = await _db.Donations
            .IgnoreQueryFilters()
            .Where(d => d.ContactId == contactId)
            .ToListAsync(ct);

        var interactions = await _db.Interactions
            .IgnoreQueryFilters()
            .Where(i => i.ContactId == contactId)
            .ToListAsync(ct);

        var documents = await _db.Documents
            .IgnoreQueryFilters()
            .Where(d => d.ContactId == contactId)
            .ToListAsync(ct);

        var consentHistory = await _consent.GetHistoryAsync(contactId, ct);

        var auditTrail = await _db.AuditLogs
            .Where(a => a.EntityName == nameof(Contact) && a.EntityId == contactId)
            .OrderByDescending(a => a.ChangedAt)
            .Take(500) // cap audit export
            .ToListAsync(ct);

        return new ContactDataExport
        {
            ExportedAt = DateTimeOffset.UtcNow,
            RequestedBy = _cu.UserId?.ToString() ?? "system",
            Subject = new ContactExportDto
            {
                Id = contact.Id,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Name = contact.Name,
                Email = contact.Email,
                Phone = contact.Phone,
                AddressLine1 = contact.AddressLine1,
                AddressLine2 = contact.AddressLine2,
                City = contact.City,
                StateProvince = contact.StateProvince,
                PostalCode = contact.PostalCode,
                Country = contact.Country,
                Tags = contact.Tags,
                Notes = contact.Notes,
                EmailOptOut = contact.EmailOptOut,
                CreatedAt = contact.CreatedAt,
                ConsentHistory = consentHistory.Select(cr => new ConsentExportDto
                {
                    ConsentType = cr.ConsentType.ToString(),
                    Status = cr.Status.ToString(),
                    PolicyVersion = cr.PolicyVersion,
                    LegalBasis = cr.LegalBasis,
                    Channel = cr.Channel,
                    RecordedBy = cr.RecordedBy,
                    RecordedAt = cr.RecordedAt
                }).ToList(),
                Donations = donations.Select(d => new DonationExportDto
                {
                    Id = d.Id,
                    Amount = d.Amount,
                    Date = d.Date,
                    Status = d.Status.ToString(),
                    PaymentMethod = d.PaymentMethod,
                    ReferenceNumber = d.ReferenceNumber
                }).ToList(),
                Interactions = interactions.Select(i => new InteractionExportDto
                {
                    Id = i.Id,
                    Type = i.Type.ToString(),
                    Body = i.Body,
                    OccurredAt = i.OccurredAt
                }).ToList(),
                Documents = documents.Select(d => new DocumentExportDto
                {
                    Id = d.Id,
                    Type = d.Type.ToString(),
                    FileName = d.FileName,
                    Available = true,
                    CreatedAt = d.CreatedAt
                }).ToList(),
                AuditTrail = auditTrail.Select(a => new AuditExportDto
                {
                    Action = a.Action,
                    Property = a.PropertyName,
                    ChangedBy = a.ChangedBy,
                    ChangedAt = a.ChangedAt
                }).ToList()
            }
        };
    }

    public async Task AnonymizeAsync(
        Guid contactId,
        string reason,
        bool retainFinancialRecords = true,
        CancellationToken ct = default)
    {
        var contact = await _db.Contacts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == contactId, ct)
            ?? throw new Application.Common.Exceptions.NotFoundException(nameof(Contact), contactId);

        var shortId = contactId.ToString("N")[..8];

        // Replace all PII with deterministic, non-reversible placeholders
        contact.FirstName = "ANON";
        contact.LastName = $"ANON-{shortId}";
        contact.Name = $"ANON-{shortId}";
        contact.Email = $"anon-{contactId:N}@deleted.invalid";
        contact.EmailHash = string.Empty; // clear the search hash
        contact.Phone = null;
        contact.AddressLine1 = null;
        contact.AddressLine2 = null;
        contact.City = null;
        contact.StateProvince = null;
        contact.PostalCode = null;
        contact.Country = null;
        contact.Notes = null;
        contact.Tags = null;
        contact.PrimaryContactName = null;
        contact.EmailOptOut = true;
        contact.LastModifiedBy = _cu.UserId?.ToString() ?? "system";
        contact.LastModifiedAt = DateTimeOffset.UtcNow;

        _db.Entry(contact).State = EntityState.Modified;

        if (!retainFinancialRecords)
        {
            // Soft-delete donations — only do this when legally permitted
            var donations = await _db.Donations
                .IgnoreQueryFilters()
                .Where(d => d.ContactId == contactId)
                .ToListAsync(ct);
            foreach (var d in donations)
            {
                d.IsDeleted = true;
                d.DeletedAt = DateTimeOffset.UtcNow;
                d.Notes = "[ERASED]";
                _db.Entry(d).State = EntityState.Modified;
            }
        }

        // Withdraw all consent types — append new withdrawal records
        var orgId = contact.OrganizationId;
        foreach (var type in Enum.GetValues<ConsentType>())
        {
            _db.Set<ConsentRecord>().Add(new ConsentRecord
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId,
                ContactId = contactId,
                ConsentType = type,
                Status = ConsentStatus.Withdrawn,
                PolicyVersion = "erasure",
                LegalBasis = "Erasure",
                Channel = "dsr-api",
                RecordedBy = _cu.UserId?.ToString() ?? "system",
                RecordedAt = DateTimeOffset.UtcNow,
                Notes = reason
            });
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Contact {ContactId} anonymized by {UserId}. Reason: {Reason}. FinancialRetained: {Retained}.",
            contactId, _cu.UserId, reason, retainFinancialRecords);
    }

    public async Task HardDeleteAsync(Guid contactId, CancellationToken ct = default)
    {
        var contact = await _db.Contacts
            .IgnoreQueryFilters()
            .Include(c => c.Donations)
            .FirstOrDefaultAsync(c => c.Id == contactId, ct)
            ?? throw new Application.Common.Exceptions.NotFoundException(nameof(Contact), contactId);

        // Safety check: block hard delete if confirmed financial records exist
        var hasFinancialRecords = contact.Donations.Any(d => !d.IsDeleted);
        if (hasFinancialRecords)
            throw new Application.Common.Exceptions.ConflictException(
                "Cannot hard-delete a contact with active donation records. " +
                "Anonymize the contact instead, or soft-delete the donations first.");

        // Soft-delete interactions (they may contain operational notes)
        var interactions = await _db.Interactions
            .IgnoreQueryFilters()
            .Where(i => i.ContactId == contactId)
            .ToListAsync(ct);
        foreach (var i in interactions)
        {
            i.IsDeleted = true;
            i.DeletedAt = DateTimeOffset.UtcNow;
            _db.Entry(i).State = EntityState.Modified;
        }

        // Hard-delete consent records (no legal reason to keep after erasure)
        var consentRecords = await _db.Set<ConsentRecord>()
            .IgnoreQueryFilters()
            .Where(cr => cr.ContactId == contactId)
            .ToListAsync(ct);
        _db.Set<ConsentRecord>().RemoveRange(consentRecords);

        // Hard-delete the contact row
        _db.Contacts.Remove(contact);

        await _db.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Contact {ContactId} HARD DELETED by {UserId}.",
            contactId, _cu.UserId);
    }
}