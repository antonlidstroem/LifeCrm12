using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Common.Behaviours;

/// <summary>
/// Marker interface for MediatR commands that require explicit consent before execution.
/// Only apply to single-contact commands (e.g. SendDirectEmail to a specific contact).
/// Bulk operations must check consent per-recipient inside the handler.
/// </summary>
public interface IRequiresConsent
{
    Guid ContactId { get; }
    ConsentType RequiredConsentType { get; }
}

/// <summary>
/// MediatR pipeline behavior. If a command implements IRequiresConsent,
/// checks consent before delegating to the handler.
/// Throws ConsentRequiredException if consent is absent.
/// </summary>
public sealed class ConsentBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IConsentService _consent;

    public ConsentBehaviour(IConsentService consent)
    {
        _consent = consent;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IRequiresConsent consentReq)
        {
            var hasConsent = await _consent.HasActiveConsentAsync(
                consentReq.ContactId,
                consentReq.RequiredConsentType,
                cancellationToken);

            if (!hasConsent)
                throw new ConsentRequiredException(
                    consentReq.ContactId,
                    consentReq.RequiredConsentType);
        }

        return await next();
    }
}