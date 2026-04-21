using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LifeCrm.Api.Hubs;

[Authorize]
public class ActivityHub : Hub
{
    public async Task JoinOrganizationGroup(string organizationId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"org-{organizationId}");

    public async Task LeaveOrganizationGroup(string organizationId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"org-{organizationId}");
}

public record ActivityEvent(string EventType, object Payload);
