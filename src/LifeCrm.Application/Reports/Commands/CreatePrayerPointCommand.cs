using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Reports.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Reports.Commands;

public class CreatePrayerPointCommand : IRequest<PrayerPointDto>
{
    public CreatePrayerPointRequest Request { get; }
    public CreatePrayerPointCommand(CreatePrayerPointRequest r) { Request = r; }
}

public sealed class CreatePrayerPointHandler : IRequestHandler<CreatePrayerPointCommand, PrayerPointDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;
    public CreatePrayerPointHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

    public async Task<PrayerPointDto> Handle(CreatePrayerPointCommand cmd, CancellationToken ct)
    {
        var orgId = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
        var r  = cmd.Request;
        var pp = new PrayerPoint { Id = Guid.NewGuid(), OrganizationId = orgId, ReportId = r.ReportId, Title = r.Title.Trim(), Detail = r.Detail.Trim(), Status = PrayerStatus.Active, CreatedBy = _cu.UserId?.ToString() ?? "system" };
        await _uow.PrayerPoints.AddAsync(pp, ct);
        await _uow.SaveChangesAsync(ct);
        return ReportMappers.MapPrayerPoint(pp);
    }
}
