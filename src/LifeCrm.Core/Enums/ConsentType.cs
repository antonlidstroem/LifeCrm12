namespace LifeCrm.Core.Enums;

/// <summary>
/// Granular consent types. Stored as individual rows in ConsentRecord
/// (one row per type) to enable clean indexing and querying.
/// NOT a [Flags] enum — each type is a separate consent record.
/// </summary>
public enum ConsentType
{
    Marketing = 1,   // Newsletters, campaigns, fundraising appeals
    Operational = 2,   // Transactional: receipts, donation confirmations
    Analytics = 3,   // Usage statistics and reporting
    ThirdParty = 4    // Sharing with partner organisations
}