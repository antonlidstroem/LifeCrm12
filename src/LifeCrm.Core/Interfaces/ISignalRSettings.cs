namespace LifeCrm.Core.Interfaces;

/// <summary>
/// Controls whether the SignalR real-time hub is active.
/// Super-admins can toggle this at runtime to reduce CPU load.
/// The application functions fully in both states.
/// </summary>
public interface ISignalRSettings
{
    Task<bool> GetEnabledAsync(CancellationToken ct = default);
    Task SetEnabledAsync(bool enabled, CancellationToken ct = default);
}
