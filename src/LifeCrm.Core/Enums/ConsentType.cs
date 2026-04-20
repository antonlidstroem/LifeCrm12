// src/LifeCrm.Core/Enums/ConsentType.cs
namespace LifeCrm.Core.Enums;

/// <summary>
/// Granular consent categories. Add new values — never remove or rename existing ones.
/// Removing a value breaks the historical record.
/// </summary>
public enum ConsentType
{
    /// <summary>Newsletters, campaigns, fundraising appeals.</summary>
    MarketingEmail = 1,

    /// <summary>Donation receipts, summaries, operational transactional email.</summary>
    TransactionalEmail = 2,

    /// <summary>Core CRM data storage (name, address, donation history).</summary>
    DataStorage = 3,

    /// <summary>Transfer of data to third-party partners or processors.</summary>
    ThirdPartySharing = 4,

    /// <summary>Profiling / segmentation based on giving history or tags.</summary>
    Profiling = 5
}