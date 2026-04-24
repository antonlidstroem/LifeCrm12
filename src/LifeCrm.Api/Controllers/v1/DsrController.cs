using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Dsr.Commands;
using LifeCrm.Application.Dsr.Queries;
using LifeCrm.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeCrm.Api.Controllers.v1;

/// <summary>
/// Data Subject Rights (DSR) endpoints.
/// All operations are admin-gated and fully audit-logged.
/// Export: FinanceOrAdmin (they handle donor communication).
/// Anonymize / Delete: AdminOnly (irreversible operations).
/// </summary>
[Authorize(Policy = "FinanceOrAdmin")]
public class DsrController : ApiControllerBase
{
    /// <summary>
    /// Exports all personal data held about a contact as a structured JSON document.
    /// Includes: contact fields, consent history, donations, interactions, documents, audit trail.
    /// Fulfill within 30 days per GDPR Art. 12.
    /// </summary>
    [HttpGet("{contactId:guid}/export")]
    [ProducesResponseType(typeof(ApiResponse<ContactDataExport>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Export(Guid contactId, CancellationToken ct)
        => OkResponse(await Mediator.Send(new ExportContactDataQuery(contactId), ct));

    /// <summary>
    /// Anonymizes a contact in place.
    /// PII fields are replaced with deterministic placeholders.
    /// Donation/financial records are retained by default (legal obligation).
    /// All consent is withdrawn.
    /// </summary>
    [HttpPost("{contactId:guid}/anonymize")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Anonymize(
        Guid contactId,
        [FromBody] AnonymizeRequest request,
        CancellationToken ct)
    {
        await Mediator.Send(new AnonymizeContactCommand(
            contactId,
            request.Reason,
            request.RetainFinancialRecords), ct);

        return NoContentResponse();
    }

    /// <summary>
    /// Permanently hard-deletes a contact and their non-financial records.
    /// Will fail if active donation records exist — anonymize first or set retainFinancialRecords=false.
    /// Requires the literal string "CONFIRM_DELETE" passed as a query parameter.
    /// </summary>
    [HttpDelete("{contactId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> HardDelete(
        Guid contactId,
        [FromQuery] string confirm,
        CancellationToken ct)
    {
        if (confirm != "CONFIRM_DELETE")
            return BadRequest(ApiResponse.Fail(
                "Hard delete requires ?confirm=CONFIRM_DELETE as a query parameter. " +
                "This operation is irreversible."));

        await Mediator.Send(new HardDeleteContactCommand(contactId), ct);
        return NoContentResponse();
    }
}

public record AnonymizeRequest
{
    public string Reason { get; init; } = string.Empty;
    public bool RetainFinancialRecords { get; init; } = true;
}