using LifeCrm.Application.Donations.Commands;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Application.Notifications;

public sealed class DonationCreatedActivityHandler : INotificationHandler<DonationCreatedNotification>
{
    private readonly IActivityNotifier _notifier;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DonationCreatedActivityHandler> _logger;
    public DonationCreatedActivityHandler(IActivityNotifier notifier, ICurrentUserService currentUser, ILogger<DonationCreatedActivityHandler> logger)
    { _notifier = notifier; _currentUser = currentUser; _logger = logger; }

    public async Task Handle(DonationCreatedNotification notification, CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue) return;
        try { await _notifier.NotifyAsync(orgId.Value.ToString(), "DonationCreated", new { donationId = notification.DonationId }, ct); }
        catch (Exception ex) { _logger.LogError(ex, "SignalR emit failed for DonationCreated {Id}.", notification.DonationId); }
    }
}
