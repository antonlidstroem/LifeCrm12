using LifeCrm.Application.Common.Exceptions;
using LifeCrm.Application.Reports.DTOs;
using LifeCrm.Core.Entities;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using MediatR;

namespace LifeCrm.Application.Reports.Commands;

public class CreateReportCommand : IRequest<Guid>
{
    public CreateReportRequest Request { get; }
    public CreateReportCommand(CreateReportRequest r) { Request = r; }
}

public sealed class CreateReportHandler : IRequestHandler<CreateReportCommand, Guid>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _cu;
    public CreateReportHandler(IUnitOfWork uow, ICurrentUserService cu) { _uow = uow; _cu = cu; }

    public async Task<Guid> Handle(CreateReportCommand cmd, CancellationToken ct)
    {
        var orgId  = _cu.OrganizationId ?? throw new ForbiddenException("No organization context.");
        var userId = _cu.UserId         ?? throw new ForbiddenException("No user context.");
        var r      = cmd.Request;
        var author     = await _uow.Users.GetByIdAsync(userId, ct);
        var authorName = author?.FullName ?? userId.ToString();
        var report = new MissionReport
        {
            Id = Guid.NewGuid(), OrganizationId = orgId, Title = r.Title.Trim(),
            ReportDate = r.ReportDate, Location = r.Location?.Trim(),
            CampaignId = r.CampaignId, ProjectId = r.ProjectId, Language = r.Language,
            Status = ReportStatus.Draft, AuthorUserId = userId, AuthorName = authorName, HtmlBody = string.Empty
        };
        await _uow.Reports.AddAsync(report, ct);
        await _uow.SaveChangesAsync(ct);
        return report.Id;
    }
}
