using MediatR;

namespace LifeCrm.Application.Reports.Commands;

public record ReportApprovedNotification(Guid ReportId, Guid AuthorUserId, string ReportTitle) : INotification;
