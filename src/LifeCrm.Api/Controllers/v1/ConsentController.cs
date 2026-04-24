using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Consent.Commands;
using LifeCrm.Application.Consent.Queries;
using LifeCrm.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeCrm.Api.Controllers.v1;

/// <summary>
/// Manages data subject consent records.
/// Recording consent: requires CanWrite (manager taking consent from a donor).
/// Viewing history: requires CanWrite.
/// Withdrawing on behalf of subject: FinanceOrAdmin (has accountability).
/// </summary>
public class ConsentController : ApiControllerBase
{
    /// <summary>
    /// Returns the full consent history for a contact, ordered newest first.
    /// </summary>
    [HttpGet("contacts/{contactId:guid}/consent")]
    [Authorize(Policy = "CanWrite")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(Guid contactId, CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetConsentHistoryQuery(contactId), ct));

    /// <summary>
    /// Record or update consent for a specific type.
    /// Appends a new ConsentRecord — previous records are never mutated.
    /// </summary>
    [HttpPost("contacts/{contactId:guid}/consent")]
    [Authorize(Policy = "CanWrite")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Record(
        Guid contactId,
        [FromBody] RecordConsentRequest request,
        CancellationToken ct)
    {
        await Mediator.Send(new RecordConsentCommand(
            contactId,
            request.ConsentType,
            request.Status,
            request.PolicyVersion,
            request.Channel,
            request.IpAddressHash), ct);

        return NoContentResponse();
    }

    /// <summary>
    /// Withdraw consent for a specific type. Creates a Withdrawn record.
    /// </summary>
    [HttpDelete("contacts/{contactId:guid}/consent/{consentType}")]
    [Authorize(Policy = "FinanceOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Withdraw(
        Guid contactId,
        ConsentType consentType,
        [FromQuery] string policyVersion = "withdrawal",
        CancellationToken ct = default)
    {
        await Mediator.Send(new RecordConsentCommand(
            contactId,
            consentType,
            ConsentStatus.Withdrawn,
            policyVersion,
            "admin-ui",
            null), ct);

        return NoContentResponse();
    }
}

public record RecordConsentRequest
{
    public ConsentType ConsentType { get; init; }
    public ConsentStatus Status { get; init; }
    public string PolicyVersion { get; init; } = string.Empty;
    public string? Channel { get; init; }
    /// <summary>Pre-hashed IP. Client must hash before sending — never send raw IP.</summary>
    public string? IpAddressHash { get; init; }
}