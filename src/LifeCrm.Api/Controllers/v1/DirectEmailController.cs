using LifeCrm.Contracts.Common.DTOs;
using LifeCrm.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace LifeCrm.Api.Controllers.v1;

/// <summary>
/// Allows Finance/Admin users to send a direct email to a contact.
/// </summary>
[Authorize(Policy = "FinanceOrAdmin")]
public class DirectEmailController : ApiControllerBase
{
    private readonly IEmailService _emailService;

    public DirectEmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Send(
        [FromBody] DirectEmailRequest request, CancellationToken ct)
    {
        try
        {
            await _emailService.SendAsync(
                request.ToEmail,
                request.ToName,
                request.Subject,
                request.HtmlBody,
                ct: ct);
            return OkResponse("Email sent successfully.");
        }
        catch (Exception ex)
        {
            // Return 200 with error message so the UI can display it
            return OkResponse($"Send failed: {ex.Message}");
        }
    }
}

public record DirectEmailRequest
{
    [Required][EmailAddress][MaxLength(320)] public string ToEmail  { get; init; } = string.Empty;
    [MaxLength(200)]                         public string ToName   { get; init; } = string.Empty;
    [Required][MaxLength(500)]               public string Subject  { get; init; } = string.Empty;
    [Required][MaxLength(100_000)]           public string HtmlBody { get; init; } = string.Empty;
}
