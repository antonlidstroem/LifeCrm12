using LifeCrm.Application.Reports.Commands;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Application.Notifications;

public sealed class ReportSubmittedActivityHandler : INotificationHandler<ReportSubmittedNotification>
{
    private readonly IActivityNotifier _notifier;
    private readonly ILogger<ReportSubmittedActivityHandler> _logger;
    public ReportSubmittedActivityHandler(IActivityNotifier notifier, ILogger<ReportSubmittedActivityHandler> logger)
    { _notifier = notifier; _logger = logger; }

    public async Task Handle(ReportSubmittedNotification n, CancellationToken ct)
    {
        try { await _notifier.NotifyAsync(n.OrgId.ToString(), "ReportSubmitted", new { reportId = n.ReportId, title = n.ReportTitle }, ct); }
        catch (Exception ex) { _logger.LogError(ex, "SignalR notify failed for ReportSubmitted {Id}.", n.ReportId); }
    }
}
