using LifeCrm.Application.Reports.DTOs;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Application.Reports.Queries;

public class GetAnsweredPrayersThisMonthQuery : IRequest<IReadOnlyList<AnsweredPrayerWidgetDto>> { }

public sealed class GetAnsweredPrayersThisMonthHandler : IRequestHandler<GetAnsweredPrayersThisMonthQuery, IReadOnlyList<AnsweredPrayerWidgetDto>>
{
    private readonly IUnitOfWork _uow;
    public GetAnsweredPrayersThisMonthHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<IReadOnlyList<AnsweredPrayerWidgetDto>> Handle(GetAnsweredPrayersThisMonthQuery q, CancellationToken ct)
    {
        var monthStart = new DateTimeOffset(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
        return await _uow.PrayerPoints.Query()
            .Include(pp => pp.Report)
            .Where(pp => pp.Status == PrayerStatus.Answered && pp.AnsweredAt.HasValue && pp.AnsweredAt >= monthStart)
            .OrderByDescending(pp => pp.AnsweredAt)
            .Select(pp => new AnsweredPrayerWidgetDto { Id = pp.Id, Title = pp.Title, AnsweredNote = pp.AnsweredNote, AnsweredAt = pp.AnsweredAt!.Value, ReportTitle = pp.Report != null ? pp.Report.Title : null, ReportId = pp.ReportId })
            .ToListAsync(ct);
    }
}
