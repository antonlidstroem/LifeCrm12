using LifeCrm.Application.Gdpr.DTOs;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Gdpr.Queries;

public record GetConsentStatusQuery(Guid ContactId) : IRequest<ConsentStatusDto>;

public sealed class GetConsentStatusHandler : IRequestHandler<GetConsentStatusQuery, ConsentStatusDto>
{
    private readonly IConsentService _consent;

    public GetConsentStatusHandler(IConsentService consent) => _consent = consent;

    public async Task<ConsentStatusDto> Handle(GetConsentStatusQuery query, CancellationToken ct)
    {
        var history = await _consent.GetHistoryAsync(query.ContactId, ct);

        // Build the "current" view: latest record per ConsentType
        var current = history
            .GroupBy(r => r.ConsentType)
            .Select(g =>
            {
                var latest = g.OrderByDescending(r => r.CreatedAt).First();
                return new ConsentSummaryDto
                {
                    ConsentType = latest.ConsentType,
                    IsGranted   = latest.IsGranted,
                    Since       = latest.CreatedAt
                };
            })
            .OrderBy(s => s.ConsentType)
            .ToList()
            .AsReadOnly();

        var historyDtos = history
            .Select(r => new ConsentEntryDto
            {
                ConsentType      = r.ConsentType,
                IsGranted        = r.IsGranted,
                PolicyVersion    = r.PolicyVersion,
                Source           = r.Source,
                RecordedAt       = r.CreatedAt,
                RecordedByUserId = r.RecordedByUserId
            })
            .ToList()
            .AsReadOnly();

        return new ConsentStatusDto
        {
            ContactId = query.ContactId,
            Current   = current,
            History   = historyDtos
        };
    }
}
