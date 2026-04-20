// src/LifeCrm.Core/Exceptions/ConsentRequiredException.cs
using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Exceptions;

public class ConsentRequiredException : Exception
{
    public Guid ContactId { get; }
    public ConsentType ConsentType { get; }

    public ConsentRequiredException(Guid contactId, ConsentType consentType)
        : base($"Contact {contactId} has not granted consent for {consentType}.")
    {
        ContactId = contactId;
        ConsentType = consentType;
    }
}