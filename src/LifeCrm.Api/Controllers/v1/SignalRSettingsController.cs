using LifeCrm.Contracts.Common.DTOs;
using LifeCrm.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeCrm.Api.Controllers.v1;

/// <summary>
/// Super-admin endpoint to toggle the SignalR real-time hub on or off.
/// Disabling reduces CPU when real-time updates are not needed.
/// The application functions fully in both states.
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class SignalRSettingsController : ApiControllerBase
{
    private readonly ISignalRSettings _settings;
    public SignalRSettingsController(ISignalRSettings settings) { _settings = settings; }

    /// <summary>Returns whether the SignalR hub is currently enabled.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
        => OkResponse(await _settings.GetEnabledAsync(ct));

    /// <summary>Enables or disables the SignalR hub. Takes effect within 30 seconds on all instances.</summary>
    [HttpPatch]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Set([FromBody] bool enabled, CancellationToken ct)
    {
        await _settings.SetEnabledAsync(enabled, ct);
        return OkResponse(enabled, enabled ? "SignalR hub enabled." : "SignalR hub disabled. Real-time updates paused.");
    }
}
