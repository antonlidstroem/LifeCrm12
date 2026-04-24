using LifeCrm.Core.Enums;

namespace LifeCrm.Application.Common.Exceptions;

/// <summary>
/// Thrown when a data subject's consent is required but absent or withdrawn.
/// Surfaces as HTTP 422 Unprocessable Entity via ExceptionMiddleware.
/// </summary>
public class ConsentRequiredException : Exception
{
    public Guid ContactId { get; }
    public ConsentType ConsentType { get; }

    public ConsentRequiredException(Guid contactId, ConsentType type)
        : base($"Contact {contactId} has not granted {type} consent. Operation blocked.")
    {
        ContactId = contactId;
        ConsentType = type;
    }
}