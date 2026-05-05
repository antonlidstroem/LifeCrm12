using LifeCrm.Contracts.Reports.DTOs;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Reports.Queries;

public class GetActivePrayerPointsQuery : IRequest<IReadOnlyList<PrayerPointDto>> { }

public sealed class GetActivePrayerPointsHandler : IRequestHandler<GetActivePrayerPointsQuery, IReadOnlyList<PrayerPointDto>>
{
    private readonly IUnitOfWork _uow;
    public GetActivePrayerPointsHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<IReadOnlyList<PrayerPointDto>> Handle(GetActivePrayerPointsQuery q, CancellationToken ct)
    {
        return await _uow.PrayerPoints.Query()
            .Where(pp => pp.Status == PrayerStatus.Active)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PrayerPointDto { Id = p.Id, ReportId = p.ReportId, Title = p.Title, Detail = p.Detail, Status = p.Status, CreatedAt = p.CreatedAt })
            .ToListAsync(ct);
    }
}
