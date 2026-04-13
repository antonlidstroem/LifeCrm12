using LifeCrm.Application.Common.DTOs;
using LifeCrm.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeCrm.Api.Controllers.v1;

/// <summary>
/// Admin endpoint to configure the outgoing (SMTP) email settings.
/// Settings are stored in the AppSettings table and override appsettings.json.
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class EmailSettingsController : ApiControllerBase
{
    private readonly IEmailSettingsService _emailSettings;
    private readonly IEmailService         _emailService;

    public EmailSettingsController(IEmailSettingsService emailSettings, IEmailService emailService)
    {
        _emailSettings = emailSettings;
        _emailService  = emailService;
    }

    /// <summary>Returns the current outgoing email configuration.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<EmailSettingsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var settings = await _emailSettings.GetAsync(ct);
        // Never return the password in plain text to the client — mask it
        var masked = settings with { Password = string.IsNullOrEmpty(settings.Password) ? "" : "••••••••" };
        return OkResponse(masked);
    }

    /// <summary>
    /// Saves new outgoing email configuration.
    /// If Password is "••••••••" (the mask), the existing password is preserved.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Save([FromBody] EmailSettingsRequest request, CancellationToken ct)
    {
        // If the client sent back the masked password, keep the existing one
        var existing = await _emailSettings.GetAsync(ct);
        var password = request.Password == "••••••••" ? existing.Password : request.Password;

        var settings = new EmailSettingsDto
        {
            Host      = request.Host.Trim(),
            Port      = request.Port,
            Username  = request.Username.Trim(),
            Password  = password,
            FromEmail = request.FromEmail.Trim(),
            FromName  = request.FromName.Trim(),
            UseSsl    = request.UseSsl,
            DryRun    = request.DryRun
        };
        await _emailSettings.SaveAsync(settings, ct);
        return NoContentResponse();
    }

    /// <summary>
    /// Sends a test email to verify the SMTP configuration works.
    /// </summary>
    [HttpPost("test")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Test([FromBody] TestEmailRequest request, CancellationToken ct)
    {
        try
        {
            await _emailService.SendAsync(
                request.ToEmail, request.ToEmail,
                "LifeCrm — Email configuration test",
                "<p>This is a test email from LifeCrm confirming that your outgoing email settings are working correctly.</p>",
                ct: ct);
            return OkResponse("Test email sent successfully.", "Check your inbox.");
        }
        catch (Exception ex)
        {
            return OkResponse($"Send failed: {ex.Message}");
        }
    }
}

public record EmailSettingsRequest
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

public record TestEmailRequest
{
    public string ToEmail { get; init; } = string.Empty;
}
