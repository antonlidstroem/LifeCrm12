using LifeCrm.Application.Contacts.Commands;
using LifeCrm.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Application.Notifications;

public sealed class ContactCreatedActivityHandler : INotificationHandler<ContactCreatedNotification>
{
    private readonly IActivityNotifier _notifier;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ContactCreatedActivityHandler> _logger;
    public ContactCreatedActivityHandler(IActivityNotifier notifier, ICurrentUserService currentUser, ILogger<ContactCreatedActivityHandler> logger)
    { _notifier = notifier; _currentUser = currentUser; _logger = logger; }

    public async Task Handle(ContactCreatedNotification notification, CancellationToken ct)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue) return;
        try { await _notifier.NotifyAsync(orgId.Value.ToString(), "ContactCreated", new { contactId = notification.ContactId, name = notification.ContactName }, ct); }
        catch (Exception ex) { _logger.LogError(ex, "SignalR emit failed for ContactCreated {Id}.", notification.ContactId); }
    }
}
