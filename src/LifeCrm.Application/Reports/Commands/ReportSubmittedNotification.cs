using MediatR;

namespace LifeCrm.Application.Reports.Commands;

public record ReportSubmittedNotification(Guid ReportId, string ReportTitle, Guid OrgId) : INotification;
