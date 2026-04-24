using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Consent.Commands;

public record RecordConsentCommand(
    Guid ContactId,
    ConsentType ConsentType,
    ConsentStatus Status,
    string PolicyVersion,
    string? Channel,
    string? IpAddressHash) : IRequest<Unit>;

public sealed class RecordConsentHandler : IRequestHandler<RecordConsentCommand, Unit>
{
    private readonly IConsentService _consent;
    private readonly IUnitOfWork _uow;

    public RecordConsentHandler(IConsentService consent, IUnitOfWork uow)
    {
        _consent = consent;
        _uow = uow;
    }

    public async Task<Unit> Handle(RecordConsentCommand cmd, CancellationToken ct)
    {
        // Verify contact exists in the current tenant (UoW applies query filter)
        _ = await _uow.Contacts.GetByIdAsync(cmd.ContactId, ct)
            ?? throw new NotFoundException(nameof(Core.Entities.Contact), cmd.ContactId);

        await _consent.RecordConsentAsync(
            cmd.ContactId,
            cmd.ConsentType,
            cmd.Status,
            cmd.PolicyVersion,
            cmd.Channel,
            cmd.IpAddressHash,
            ct);

        return Unit.Value;
    }
}