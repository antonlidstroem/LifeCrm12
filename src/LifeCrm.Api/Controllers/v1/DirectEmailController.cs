using System.ComponentModel.DataAnnotations;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Core.Enums;
using LifeCrm.Core.Interfaces;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Api.Controllers.v1;

/// <summary>
/// Allows Finance/Admin users to send a direct email to a contact.
/// Enforces TransactionalEmail consent before sending.
/// </summary>
[Authorize(Policy = "FinanceOrAdmin")]
public class DirectEmailController : ApiControllerBase
{
    private readonly IEmailService _emailService;
    private readonly IConsentService _consent;
    private readonly AppDbContext _db;

    public DirectEmailController(
        IEmailService emailService,
        IConsentService consent,
        AppDbContext db)
    {
        _emailService = emailService;
        _consent      = consent;
        _db           = db;
    }

    [HttpPost("send")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Send(
        [FromBody] DirectEmailRequest request, CancellationToken ct)
    {
        // Resolve contact by email to enforce consent
        if (request.ContactId.HasValue)
        {
            var hasConsent = await _consent.HasConsentAsync(
                request.ContactId.Value, ConsentType.TransactionalEmail, ct);

            if (!hasConsent)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<string>.Fail(
                        $"Contact {request.ContactId} has not granted TransactionalEmail consent. " +
                        "Record consent before sending direct email."));
            }

            // Also check EmailOptOut as a belt-and-suspenders guard
            var contact = await _db.Contacts
                .Where(c => c.Id == request.ContactId.Value)
                .Select(c => new { c.EmailOptOut, c.IsAnonymized })
                .FirstOrDefaultAsync(ct);

            if (contact?.IsAnonymized == true)
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<string>.Fail("Cannot send email to an anonymized contact."));

            if (contact?.EmailOptOut == true)
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<string>.Fail(
                        "Contact has opted out of email communications. " +
                        "Withdraw their opt-out before sending."));
        }

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
            return OkResponse($"Send failed: {ex.Message}");
        }
    }
}

public record DirectEmailRequest
{
    /// <summary>
    /// If provided, consent is checked before sending.
    /// Strongly recommended — omitting this bypasses the consent gate.
    /// </summary>
    public Guid? ContactId { get; init; }

    [Required][EmailAddress][MaxLength(320)] public string ToEmail  { get; init; } = string.Empty;
    [MaxLength(200)]                         public string ToName   { get; init; } = string.Empty;
    [Required][MaxLength(500)]               public string Subject  { get; init; } = string.Empty;
    [Required][MaxLength(100_000)]           public string HtmlBody { get; init; } = string.Empty;
}
