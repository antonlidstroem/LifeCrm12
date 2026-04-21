using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Interfaces;

public interface IConsentService
{
    /// <summary>
    /// Returns true if the contact has an active grant for the given consent type.
    /// Evaluates the most recent ConsentRecord for this contact+type pair.
    /// </summary>
    Task<bool> HasConsentAsync(
        Guid contactId,
        ConsentType consentType,
        CancellationToken ct = default);

    /// <summary>
    /// Returns a snapshot of current (latest) consent state for every ConsentType.
    /// </summary>
    Task<IReadOnlyDictionary<ConsentType, bool>> GetCurrentConsentsAsync(
        Guid contactId,
        CancellationToken ct = default);

    /// <summary>
    /// Throws ConsentRequiredException if consent is absent.
    /// Call this in command handlers before sending any communication to a contact.
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

    /// <summary>Full consent history for a contact — used by DSR export.</summary>
    Task<IReadOnlyList<ConsentRecord>> GetHistoryAsync(
        Guid contactId,
        CancellationToken ct = default);
}
