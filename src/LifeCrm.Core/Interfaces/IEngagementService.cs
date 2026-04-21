namespace LifeCrm.Core.Interfaces;

public interface IEngagementService
{
    /// <summary>
    /// Returns a computed engagement summary for a contact.
    /// All values are derived at query time — nothing is stored.
    /// </summary>
    Task<EngagementSummary> GetSummaryAsync(Guid contactId, CancellationToken ct = default);
}

public record EngagementSummary
{
    public int     TotalEvents       { get; init; }
    public int     TotalDonations    { get; init; }
    public int     TotalInteractions { get; init; }
    public decimal TotalDonated      { get; init; }

    public DateTimeOffset? LastEventAt     { get; init; }
    public DateTimeOffset? LastDonationAt  { get; init; }

    /// <summary>
    /// Derived segment — never stored, computed fresh on each call.
    /// New | Growing | Core | AtRisk | Lapsed
    /// </summary>
    public string EngagementSegment  { get; init; } = "New";
}
