// src/LifeCrm.Application/Reports/Commands/DeleteReportCommand.cs
using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Reports.Commands;

public class DeleteReportCommand : IRequest<Unit>
{
    public Guid ReportId { get; }
    public bool IsAdmin { get; }
    public DeleteReportCommand(Guid id, bool isAdmin = false)
    {
        ReportId = id;
        IsAdmin = isAdmin;
    }
}

public sealed class DeleteReportHandler : IRequestHandler<DeleteReportCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    public DeleteReportHandler(IUnitOfWork uow) { _uow = uow; }

    public async Task<Unit> Handle(DeleteReportCommand cmd, CancellationToken ct)
    {
        var rpt = await _uow.Reports.GetByIdAsync(cmd.ReportId, ct)
            ?? throw new NotFoundException(nameof(MissionReport), cmd.ReportId);

        // Admins can delete any report including Approved ones
        if (rpt.Status == ReportStatus.Approved && !cmd.IsAdmin)
            throw new ConflictException("Only admins can delete approved reports.");

        _uow.Reports.Delete(rpt);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}