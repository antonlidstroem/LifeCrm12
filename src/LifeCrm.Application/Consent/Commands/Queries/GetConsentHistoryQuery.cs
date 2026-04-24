using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Consent.Queries;

public record GetConsentHistoryQuery(Guid ContactId) : IRequest<IReadOnlyList<ConsentRecord>>;

public sealed class GetConsentHistoryHandler
    : IRequestHandler<GetConsentHistoryQuery, IReadOnlyList<ConsentRecord>>
{
    private readonly IConsentService _consent;
    private readonly IUnitOfWork _uow;

    public GetConsentHistoryHandler(IConsentService consent, IUnitOfWork uow)
    {
        _consent = consent;
        _uow = uow;
    }

    public async Task<IReadOnlyList<ConsentRecord>> Handle(
        GetConsentHistoryQuery query, CancellationToken ct)
    {
        _ = await _uow.Contacts.GetByIdAsync(query.ContactId, ct)
            ?? throw new NotFoundException(nameof(Core.Entities.Contact), query.ContactId);

        return await _consent.GetHistoryAsync(query.ContactId, ct);
    }
}