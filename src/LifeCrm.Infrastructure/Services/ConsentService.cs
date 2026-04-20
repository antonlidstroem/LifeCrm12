// src/LifeCrm.Infrastructure/Services/ConsentService.cs
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Exceptions;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;

namespace LifeCrm.Infrastructure.Services;

public class ConsentService : IConsentService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ConsentService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<bool> HasConsentAsync(
        Guid contactId, ConsentType consentType, CancellationToken ct = default)
    {
        // Most recent record for this contact+type determines current state
        var latest = await _db.ConsentRecords
            .Where(r => r.ContactId == consentType.GetHashCode() // below: correct query
                     && r.ContactId == contactId
                     && r.ConsentType == consentType)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => (bool?)r.IsGranted)
            .FirstOrDefaultAsync(ct);

        return latest == true;
    }

    public async Task RequireConsentAsync(
        Guid contactId, ConsentType consentType, CancellationToken ct = default)
    {
        if (!await HasConsentAsync(contactId, consentType, ct))
            throw new ConsentRequiredException(contactId, consentType);
    }

    public async Task GrantAsync(
        Guid contactId, ConsentType consentType, string policyVersion,
        string source, Guid? recordedByUserId = null,
        string? ipAddress = null, CancellationToken ct = default)
    {
        _db.ConsentRecords.Add(new ConsentRecord
        {
            Id = Guid.NewGuid(),
            ContactId = contactId,
            ConsentType = consentType,
            IsGranted = true,
            PolicyVersion = policyVersion,
            Source = source,
            RecordedByUserId = recordedByUserId ?? _currentUser.UserId,
            IpAddress = ipAddress,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task WithdrawAsync(
        Guid contactId, ConsentType consentType,
        string source, Guid? recordedByUserId = null, CancellationToken ct = default)
    {
        _db.ConsentRecords.Add(new ConsentRecord
        {
            Id = Guid.NewGuid(),
            ContactId = contactId,
            ConsentType = consentType,
            IsGranted = false,
            PolicyVersion = "N/A",
            Source = source,
            RecordedByUserId = recordedByUserId ?? _currentUser.UserId,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ConsentRecord>> GetHistoryAsync(
        Guid contactId, CancellationToken ct = default)
        => await _db.ConsentRecords
            .Where(r => r.ContactId == contactId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
}