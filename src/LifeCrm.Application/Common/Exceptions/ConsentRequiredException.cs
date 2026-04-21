using LifeCrm.Core.Enums;

namespace LifeCrm.Application.Common.Exceptions;

/// <summary>
/// Thrown when an operation requires a specific consent type that the contact has not granted.
/// Maps to HTTP 403 in ExceptionMiddleware.
/// </summary>
public class ConsentRequiredException : Exception
{
    public Guid        ContactId   { get; }
    public ConsentType ConsentType { get; }

    public ConsentRequiredException(Guid contactId, ConsentType consentType)
        : base($"Contact {contactId} has not granted consent for {consentType}.")
    {
        ContactId   = contactId;
        ConsentType = consentType;
    }
}
