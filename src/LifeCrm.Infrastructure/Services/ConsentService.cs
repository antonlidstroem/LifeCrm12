using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Infrastructure.Services;

public class ConsentService : IConsentService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _cu;

    public ConsentService(AppDbContext db, ICurrentUserService cu)
    {
        _db = db;
        _cu = cu;
    }

    public async Task<bool> HasActiveConsentAsync(
        Guid contactId, ConsentType type, CancellationToken ct = default)
    {
        var latest = await _db.Set<ConsentRecord>()
            .IgnoreQueryFilters()
            .Where(cr => cr.ContactId == contactId
                      && cr.ConsentType == type
                      && !cr.IsDeleted)
            .OrderByDescending(cr => cr.RecordedAt)
            .FirstOrDefaultAsync(ct);

        return latest?.Status == ConsentStatus.Granted;
    }

    public async Task<bool> HasConsentForVersionAsync(
        Guid contactId, ConsentType type, string policyVersion, CancellationToken ct = default)
    {
        var latest = await _db.Set<ConsentRecord>()
            .IgnoreQueryFilters()
            .Where(cr => cr.ContactId == contactId
                      && cr.ConsentType == type
                      && cr.PolicyVersion == policyVersion
                      && !cr.IsDeleted)
            .OrderByDescending(cr => cr.RecordedAt)
            .FirstOrDefaultAsync(ct);

        return latest?.Status == ConsentStatus.Granted;
    }

    public async Task RecordConsentAsync(
        Guid contactId,
        ConsentType type,
        ConsentStatus status,
        string policyVersion,
        string? channel = null,
        string? ipAddressHash = null,
        CancellationToken ct = default)
    {
        var orgId = _cu.OrganizationId
            ?? throw new InvalidOperationException("Cannot record consent without organisation context.");

        var record = new ConsentRecord
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            ContactId = contactId,
            ConsentType = type,
            Status = status,
            PolicyVersion = policyVersion,
            LegalBasis = status == ConsentStatus.Granted ? "Consent" : "Withdrawal",
            Channel = channel,
            IpAddressHash = ipAddressHash,
            RecordedBy = _cu.UserId?.ToString() ?? "system",
            RecordedAt = DateTimeOffset.UtcNow
        };

        _db.Set<ConsentRecord>().Add(record);

        // Keep legacy EmailOptOut in sync for backward compatibility
        if (type == ConsentType.Marketing)
        {
            var contact = await _db.Contacts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == contactId, ct);
            if (contact is not null)
            {
                contact.EmailOptOut = status != ConsentStatus.Granted;
                _db.Entry(contact).State = EntityState.Modified;
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ConsentRecord>> GetHistoryAsync(
        Guid contactId, CancellationToken ct = default)
    {
        return await _db.Set<ConsentRecord>()
            .IgnoreQueryFilters()
            .Where(cr => cr.ContactId == contactId && !cr.IsDeleted)
            .OrderByDescending(cr => cr.RecordedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyDictionary<ConsentType, ConsentStatus>> GetCurrentStatesAsync(
        Guid contactId, CancellationToken ct = default)
    {
        var allTypes = Enum.GetValues<ConsentType>();
        var result = new Dictionary<ConsentType, ConsentStatus>();

        // One query — group by type, take latest per type
        var latestPerType = await _db.Set<ConsentRecord>()
            .IgnoreQueryFilters()
            .Where(cr => cr.ContactId == contactId && !cr.IsDeleted)
            .GroupBy(cr => cr.ConsentType)
            .Select(g => new
            {
                Type = g.Key,
                Status = g.OrderByDescending(cr => cr.RecordedAt).First().Status
            })
            .ToListAsync(ct);

        foreach (var type in allTypes)
        {
            var found = latestPerType.FirstOrDefault(x => x.Type == type);
            result[type] = found?.Status ?? ConsentStatus.Withdrawn; // default = no consent
        }

        return result;
    }
}