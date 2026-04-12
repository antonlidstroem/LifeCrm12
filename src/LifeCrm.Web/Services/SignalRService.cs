using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace LifeCrm.Web.Services;

public sealed class SignalRService : IAsyncDisposable
{
    private readonly ILogger<SignalRService> _logger;
    private HubConnection? _hub;
    private string _orgId = string.Empty;

    public event Func<ActivityEvent, Task>? OnActivity;
    public bool IsConnected => _hub?.State == HubConnectionState.Connected;

    public SignalRService(ILogger<SignalRService> logger) { _logger = logger; }

    /// <summary>
    /// Connects to the SignalR hub. If the server has disabled SignalR,
    /// the hub will abort the connection cleanly and this method returns silently.
    /// </summary>
    public async Task ConnectAsync(string baseUrl, string jwtToken, string orgId, bool signalREnabled = true)
    {
        if (!signalREnabled)
        {
            _logger.LogInformation("SignalR is disabled by admin setting. Skipping connection.");
            return;
        }

        if (_hub is not null) await DisconnectAsync();
        _orgId = orgId;

        _hub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl.TrimEnd('/')}/hubs/activity", opts =>
                opts.AccessTokenProvider = () => Task.FromResult<string?>(jwtToken))
            .WithAutomaticReconnect()
            .Build();

        _hub.On<ActivityEvent>("activity", async evt => { if (OnActivity is not null) await OnActivity.Invoke(evt); });
        _hub.Closed += ex => { _logger.LogWarning(ex, "SignalR connection closed."); return Task.CompletedTask; };
        _hub.Reconnected += async connectionId =>
        {
            var hub = _hub;
            if (hub is null) return;
            try { await hub.InvokeAsync("JoinOrganization", _orgId); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to re-join org group after reconnect."); }
        };

        try
        {
            await _hub.StartAsync();
            await _hub.InvokeAsync("JoinOrganization", _orgId);
            _logger.LogInformation("SignalR connected and joined org group {OrgId}.", _orgId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR connection failed — real-time feed disabled.");
            // Do not rethrow — SignalR failure must never crash the app
        }
    }

    public async Task DisconnectAsync()
    {
        if (_hub is null) return;
        try { await _hub.StopAsync(); } catch { }
        await _hub.DisposeAsync();
        _hub = null;
    }

    public async ValueTask DisposeAsync() => await DisconnectAsync();
}

public record ActivityEvent
{
    public string  EventType { get; init; } = string.Empty;
    public object? Payload   { get; init; }
}
