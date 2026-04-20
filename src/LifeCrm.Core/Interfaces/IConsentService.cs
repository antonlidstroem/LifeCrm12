// src/LifeCrm.Core/Interfaces/IConsentService.cs
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Interfaces;

public interface IConsentService
{
    /// <summary>
    /// Returns true if the contact has an active grant for the given consent type.
    /// Checks the most recent ConsentRecord for this contact+type combination.
    /// </summary>
    Task<bool> HasConsentAsync(
        Guid contactId,
        ConsentType consentType,
        CancellationToken ct = default);

    /// <summary>
    /// Throws ConsentRequiredException if consent is absent.
    /// Use in command handlers before sending any communication.
    /// </summary>
    Task RequireConsentAsync(
        Guid contactId,
        ConsentType consentType,
        CancellationToken ct = default);

    Task GrantAsync(
        Guid contactId,
        ConsentType consentType,
        string policyVersion,
        string source,
        Guid? recordedByUserId = null,
        string? ipAddress = null,
        CancellationToken ct = default);

    Task WithdrawAsync(
        Guid contactId,
        ConsentType consentType,
        string source,
        Guid? recordedByUserId = null,
        CancellationToken ct = default);

    /// <summary>Full consent history for DSR export.</summary>
    Task<IReadOnlyList<ConsentRecord>> GetHistoryAsync(
        Guid contactId,
        CancellationToken ct = default);
}