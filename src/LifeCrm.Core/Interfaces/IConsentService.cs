using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Interfaces;

public interface IConsentService
{
    /// <summary>
    /// Returns true if the contact has an active (Granted) consent record
    /// for the specified type, regardless of policy version.
    /// </summary>
    Task<bool> HasActiveConsentAsync(
        Guid contactId, ConsentType type, CancellationToken ct = default);

    /// <summary>
    /// Returns true only if the contact has granted consent under the specific policy version.
    /// Use this when re-soliciting consent after a policy update.
    /// </summary>
    Task<bool> HasConsentForVersionAsync(
        Guid contactId, ConsentType type, string policyVersion, CancellationToken ct = default);

    /// <summary>
    /// Appends a new consent record. Never mutates existing records.
    /// </summary>
    Task RecordConsentAsync(
        Guid contactId,
        ConsentType type,
        ConsentStatus status,
        string policyVersion,
        string? channel = null,
        string? ipAddressHash = null,
        CancellationToken ct = default);

    /// <summary>
    /// Full consent history for a contact, ordered newest-first.
    /// Used for DSR subject access requests.
    /// </summary>
    Task<IReadOnlyList<ConsentRecord>> GetHistoryAsync(
        Guid contactId, CancellationToken ct = default);

    /// <summary>
    /// Current effective consent state per type for a contact.
    /// Returns the latest record per ConsentType.
    /// </summary>
    Task<IReadOnlyDictionary<ConsentType, ConsentStatus>> GetCurrentStatesAsync(
        Guid contactId, CancellationToken ct = default);
}