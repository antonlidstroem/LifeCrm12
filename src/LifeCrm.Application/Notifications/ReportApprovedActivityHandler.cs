using LifeCrm.Application.Reports.Commands;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Application.Notifications;

public sealed class ReportApprovedActivityHandler : INotificationHandler<ReportApprovedNotification>
{
    private readonly IActivityNotifier _notifier;
    private readonly ICurrentUserService _cu;
    private readonly ILogger<ReportApprovedActivityHandler> _logger;
    public ReportApprovedActivityHandler(IActivityNotifier notifier, ICurrentUserService cu, ILogger<ReportApprovedActivityHandler> logger)
    { _notifier = notifier; _cu = cu; _logger = logger; }

    public async Task Handle(ReportApprovedNotification n, CancellationToken ct)
    {
        var orgId = _cu.OrganizationId;
        if (!orgId.HasValue) return;
        try { await _notifier.NotifyAsync(orgId.Value.ToString(), "ReportApproved", new { reportId = n.ReportId, title = n.ReportTitle, authorId = n.AuthorUserId }, ct); }
        catch (Exception ex) { _logger.LogError(ex, "SignalR notify failed for ReportApproved {Id}.", n.ReportId); }
    }
}
