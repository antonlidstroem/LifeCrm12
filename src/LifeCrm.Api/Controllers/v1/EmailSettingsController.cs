using LifeCrm.Application.Common.DTOs;
using LifeCrm.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LifeCrm.Api.Controllers.v1;

/// <summary>
/// Admin-only endpoint to configure outgoing (SMTP) email settings.
/// Stores configuration in the AppSettings DB table, overriding appsettings.json.
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class EmailSettingsController : ApiControllerBase
{
    private readonly IEmailSettingsService _emailSettings;
    private readonly IEmailService         _emailService;

    public EmailSettingsController(
        IEmailSettingsService emailSettings,
        IEmailService         emailService)
    {
        _emailSettings = emailSettings;
        _emailService  = emailService;
    }

    /// <summary>Returns the current SMTP configuration (password is masked).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<EmailSettingsResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var s = await _emailSettings.GetAsync(ct);
        return OkResponse(new EmailSettingsResponse
        {
            Host      = s.Host,
            Port      = s.Port,
            Username  = s.Username,
            // Never send the real password to the client
            Password  = string.IsNullOrEmpty(s.Password) ? string.Empty : "••••••••",
            FromEmail = s.FromEmail,
            FromName  = s.FromName,
            UseSsl    = s.UseSsl,
            DryRun    = s.DryRun
        });
    }

    /// <summary>
    /// Saves SMTP configuration.
    /// If the client sends back the masked password "••••••••", the existing password is preserved.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Save(
        [FromBody] EmailSettingsSaveRequest request, CancellationToken ct)
    {
        // If client sent the mask back, keep the existing stored password
        var existing = await _emailSettings.GetAsync(ct);
        var password = request.Password == "••••••••" ? existing.Password : request.Password;

        await _emailSettings.SaveAsync(new EmailSettingsDto
        {
            Host      = request.Host.Trim(),
            Port      = request.Port,
            Username  = request.Username.Trim(),
            Password  = password,
            FromEmail = request.FromEmail.Trim(),
            FromName  = request.FromName.Trim(),
            UseSsl    = request.UseSsl,
            DryRun    = request.DryRun
        }, ct);

        return NoContentResponse();
    }

    /// <summary>Sends a test email using the current configuration to verify it works.</summary>
    [HttpPost("test")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Test(
        [FromBody] TestEmailRequest request, CancellationToken ct)
    {
        try
        {
            await _emailService.SendAsync(
                request.ToEmail, request.ToEmail,
                "LifeCrm — Email configuration test",
                "<p>This is a test email from <strong>LifeCrm</strong>.<br/>" +
                "Your outgoing email settings are working correctly.</p>",
                ct: ct);
            return OkResponse("Test email sent successfully. Check your inbox.");
        }
        catch (Exception ex)
        {
            return OkResponse($"Send failed: {ex.Message}");
        }
    }
}

// ── Request / Response records ───────────────────────────────────────────────

public record EmailSettingsResponse
{
    public string Host      { get; init; } = string.Empty;
    public int    Port      { get; init; } = 587;
    public string Username  { get; init; } = string.Empty;
    public string Password  { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName  { get; init; } = string.Empty;
    public bool   UseSsl    { get; init; } = true;
    public bool   DryRun    { get; init; } = false;
}

public record EmailSettingsSaveRequest
{
    [Required] public string Host      { get; init; } = string.Empty;
    [Range(1, 65535)] public int Port  { get; init; } = 587;
    public string Username  { get; init; } = string.Empty;
    public string Password  { get; init; } = string.Empty;
    [Required][EmailAddress] public string FromEmail { get; init; } = string.Empty;
    [Required] public string FromName  { get; init; } = string.Empty;
    public bool UseSsl { get; init; } = true;
    public bool DryRun { get; init; } = false;
}

public record TestEmailRequest
{
    [Required][EmailAddress] public string ToEmail { get; init; } = string.Empty;
}
