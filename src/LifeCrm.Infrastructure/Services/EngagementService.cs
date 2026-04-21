using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Infrastructure.Services;

/// <summary>
/// Computes engagement summaries purely from existing data — nothing is stored.
/// The segment classification is descriptive only; it informs human decisions,
/// never triggers automated actions.
/// </summary>
public class EngagementService : IEngagementService
{
    private readonly AppDbContext _db;

    public EngagementService(AppDbContext db) => _db = db;

    public async Task<EngagementSummary> GetSummaryAsync(Guid contactId, CancellationToken ct = default)
    {
        var eventCount = await _db.EventAttendances
            .CountAsync(a => a.ContactId == contactId, ct);

        var lastEventAt = await _db.EventAttendances
            .Where(a => a.ContactId == contactId)
            .Include(a => a.Event)
            .OrderByDescending(a => a.Event!.StartsAt)
            .Select(a => (DateTimeOffset?)a.Event!.StartsAt)
            .FirstOrDefaultAsync(ct);

        var donations = await _db.Donations
            .Where(d => d.ContactId == contactId)
            .Select(d => new { d.Amount, d.Date })
            .ToListAsync(ct);

        var interactionCount = await _db.Interactions
            .CountAsync(i => i.ContactId == contactId, ct);

        var totalDonated   = donations.Sum(d => d.Amount);
        var lastDonationAt = donations
            .OrderByDescending(d => d.Date)
            .Select(d => (DateTimeOffset?)new DateTimeOffset(d.Date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero))
            .FirstOrDefault();

        var totalEngagements = eventCount + donations.Count;
        var mostRecentEngagement = new[] { lastEventAt, lastDonationAt }
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .OrderDescending()
            .Cast<DateTimeOffset?>()
            .FirstOrDefault();

        return new EngagementSummary
        {
            TotalEvents       = eventCount,
            TotalDonations    = donations.Count,
            TotalInteractions = interactionCount,
            TotalDonated      = totalDonated,
            LastEventAt       = lastEventAt,
            LastDonationAt    = lastDonationAt,
            EngagementSegment = Classify(totalEngagements, mostRecentEngagement)
        };
    }

    private static string Classify(int total, DateTimeOffset? lastEngagement)
    {
        var days = lastEngagement.HasValue
            ? (DateTimeOffset.UtcNow - lastEngagement.Value).TotalDays
            : double.MaxValue;

        return (total, days) switch
        {
            (0, _)        => "Lapsed",
            (<= 1, _)     => "New",
            (_, > 180)    => "Lapsed",
            (_, > 60)     => "AtRisk",
            (>= 6, <= 60) => "Core",
            _             => "Growing"
        };
    }
}
