using LifeCrm.Contracts.Reports.DTOs;
using LifeCrm.Core.Entities;

namespace LifeCrm.Application.Reports.Commands;

internal static class ReportMappers
{
    internal static PrayerPointDto MapPrayerPoint(PrayerPoint pp) => new()
    {
        Id = pp.Id, ReportId = pp.ReportId, Title = pp.Title, Detail = pp.Detail,
        Status = pp.Status, AnsweredAt = pp.AnsweredAt, AnsweredNote = pp.AnsweredNote, CreatedAt = pp.CreatedAt
    };
}
