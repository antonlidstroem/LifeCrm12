using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Gdpr.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Gdpr.Commands;

public record UpdateConsentCommand(
    Guid ContactId,
    UpdateConsentRequest Request) : IRequest<ConsentEntryDto>;

public sealed class UpdateConsentHandler : IRequestHandler<UpdateConsentCommand, ConsentEntryDto>
{
    private readonly IConsentService _consent;
    private readonly ICurrentUserService _cu;

    public UpdateConsentHandler(IConsentService consent, ICurrentUserService cu)
    {
        _consent = consent;
        _cu      = cu;
    }

    public async Task<ConsentEntryDto> Handle(UpdateConsentCommand cmd, CancellationToken ct)
    {
        var req    = cmd.Request;
        var userId = _cu.UserId;
        var now    = DateTimeOffset.UtcNow;

        if (req.IsGranted)
        {
            await _consent.GrantAsync(
                cmd.ContactId, req.ConsentType, req.PolicyVersion,
                req.Source, userId, req.IpAddress, ct);
        }
        else
        {
            await _consent.WithdrawAsync(
                cmd.ContactId, req.ConsentType, req.Source, userId, ct);
        }

        return new ConsentEntryDto
        {
            ConsentType      = req.ConsentType,
            IsGranted        = req.IsGranted,
            PolicyVersion    = req.PolicyVersion,
            Source           = req.Source,
            RecordedAt       = now,
            RecordedByUserId = userId
        };
    }
}
