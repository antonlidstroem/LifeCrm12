using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Contracts.Reports.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Reports.Commands;

public class MarkPrayerAnsweredCommand : IRequest<PrayerPointDto>
{
    public Guid PrayerPointId { get; }
    public MarkAnsweredRequest Request { get; }
    public MarkPrayerAnsweredCommand(Guid id, MarkAnsweredRequest r) { PrayerPointId = id; Request = r; }
}

public sealed class MarkPrayerAnsweredHandler : IRequestHandler<MarkPrayerAnsweredCommand, PrayerPointDto>
{
    private readonly IUnitOfWork _uow;
    public MarkPrayerAnsweredHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<PrayerPointDto> Handle(MarkPrayerAnsweredCommand cmd, CancellationToken ct)
    {
        var pp = await _uow.PrayerPoints.GetByIdAsync(cmd.PrayerPointId, ct)
            ?? throw new NotFoundException(nameof(PrayerPoint), cmd.PrayerPointId);
        pp.Status = PrayerStatus.Answered; pp.AnsweredAt = DateTimeOffset.UtcNow; pp.AnsweredNote = cmd.Request.AnsweredNote?.Trim();
        _uow.PrayerPoints.Update(pp);
        await _uow.SaveChangesAsync(ct);
        return ReportMappers.MapPrayerPoint(pp);
    }
}
