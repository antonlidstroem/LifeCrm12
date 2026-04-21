using Microsoft.AspNetCore.SignalR.Client;

namespace LifeCrm.Web.Services;

public class SignalRService : IAsyncDisposable
{
    private HubConnection? _hub;
    public event Func<ActivityEvent, Task>? OnActivity;

    public async Task ConnectAsync(string baseUrl, string token, string orgId, bool enabled)
    {
        if (!enabled) return;
        try
        {
            _hub = new HubConnectionBuilder()
                .WithUrl($"{baseUrl}/hubs/activity?access_token={token}")
                .WithAutomaticReconnect()
                .Build();

            _hub.On<string, object>("ActivityReceived", (eventType, payload) =>
            {
                OnActivity?.Invoke(new ActivityEvent(eventType, payload));
                return Task.CompletedTask;
            });

            await _hub.StartAsync();
            await _hub.InvokeAsync("JoinOrganizationGroup", orgId);
        }
        catch { /* SignalR is optional — swallow connection errors */ }
    }

    public async Task DisconnectAsync()
    {
        if (_hub is not null)
        {
            await _hub.StopAsync();
            await _hub.DisposeAsync();
            _hub = null;
        }
    }

    public async ValueTask DisposeAsync() => await DisconnectAsync();
}

public record ActivityEvent(string EventType, object Payload);
