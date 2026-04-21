using System.IO.Compression;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Gdpr.Commands;
using LifeCrm.Application.Gdpr.DTOs;
using LifeCrm.Application.Gdpr.Queries;
using LifeCrm.Core.Entities;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Api.Controllers.v1;

/// <summary>
/// GDPR data subject rights endpoints.
///
/// Access model:
///   Consent read/write  → FinanceOrAdmin
///   DSR export request  → FinanceOrAdmin
///   Anonymize           → AdminOnly
///   Hard delete         → AdminOnly
/// </summary>
[Route("api/v1/gdpr")]
[ApiController]
[Authorize]
public class GdprController : ApiControllerBase
{
    private readonly AppDbContext _db;

    public GdprController(AppDbContext db) => _db = db;

    // ── Consent ───────────────────────────────────────────────────────────────

    /// <summary>Returns current consent status and full history for a contact.</summary>
    [HttpGet("contacts/{contactId:guid}/consent")]
    [Authorize(Policy = "FinanceOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<ConsentStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConsent(Guid contactId, CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetConsentStatusQuery(contactId), ct));

    /// <summary>Records a consent grant or withdrawal for a specific consent type.</summary>
    [HttpPost("contacts/{contactId:guid}/consent")]
    [Authorize(Policy = "FinanceOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<ConsentEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateConsent(
        Guid contactId,
        [FromBody] UpdateConsentRequest request,
        CancellationToken ct)
        => OkResponse(await Mediator.Send(new UpdateConsentCommand(contactId, request), ct));

    // ── DSR Export ────────────────────────────────────────────────────────────

    /// <summary>
    /// Initiates an async export of all data held for a contact.
    /// Returns 202 Accepted with a job ID. Poll the status URL for completion.
    /// </summary>
    [HttpPost("contacts/{contactId:guid}/export")]
    [Authorize(Policy = "FinanceOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<DsrExportJobDto>), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RequestExport(Guid contactId, CancellationToken ct)
    {
        var job = await Mediator.Send(new RequestDsrExportCommand(contactId), ct);
        return StatusCode(StatusCodes.Status202Accepted, ApiResponse<DsrExportJobDto>.Ok(job));
    }

    /// <summary>Returns the current status of a DSR export job.</summary>
    [HttpGet("export-jobs/{jobId:guid}")]
    [Authorize(Policy = "FinanceOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<DsrExportJobDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExportJobStatus(Guid jobId, CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetDsrExportJobQuery(jobId), ct));

    /// <summary>
    /// Downloads the completed export as a GZip-compressed JSON file.
    /// The download link expires 24 hours after job completion.
    /// </summary>
    [HttpGet("export-jobs/{jobId:guid}/download")]
    [Authorize(Policy = "FinanceOrAdmin")]
    [Produces("application/json")]
    public async Task<IActionResult> DownloadExport(Guid jobId, CancellationToken ct)
    {
        var job = await _db.Set<DsrExportJob>()
            .FirstOrDefaultAsync(j => j.Id == jobId, ct);

        if (job is null)
            return NotFoundResponse($"Export job {jobId} not found.");

        if (job.Status != DsrExportStatus.Completed || job.ExportPayload is null)
            return BadRequest(ApiResponse.Fail($"Export job is not ready (status: {job.Status})."));

        if (job.ExpiresAt.HasValue && job.ExpiresAt.Value < DateTimeOffset.UtcNow)
            return BadRequest(ApiResponse.Fail("Export link has expired. Request a new export."));

        // Decompress and serve as JSON
        using var input  = new MemoryStream(job.ExportPayload);
        using var gz     = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        await gz.CopyToAsync(output, ct);

        var fileName = $"dsr-export-{job.ContactId:N}-{job.CompletedAt:yyyyMMdd}.json";
        return File(output.ToArray(), "application/json", fileName);
    }

    // ── Anonymization ─────────────────────────────────────────────────────────

    /// <summary>
    /// Anonymizes all PII for a contact in-place.
    /// The contact row is retained for referential integrity (Donations, ConsentRecords).
    /// Associated Document PDF blobs are hard-deleted (cannot be anonymized in binary form).
    /// Interaction bodies are replaced with [Anonymized].
    /// </summary>
    [HttpPost("contacts/{contactId:guid}/anonymize")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<AnonymizeContactResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Anonymize(Guid contactId, CancellationToken ct)
        => OkResponse(await Mediator.Send(new AnonymizeContactCommand(contactId), ct));

    // ── Hard Delete ───────────────────────────────────────────────────────────

    /// <summary>
    /// Permanently deletes a contact and all associated data.
    /// Use only when there is no legal obligation to retain the records
    /// (e.g. no confirmed donations). In most cases, prefer /anonymize.
    /// Requires explicit confirmation header to prevent accidental calls.
    /// </summary>
    [HttpDelete("contacts/{contactId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> HardDelete(
        Guid contactId,
        [FromHeader(Name = "X-Gdpr-Confirm")] string? confirm,
        CancellationToken ct)
    {
        if (confirm != "HARD-DELETE")
            return BadRequest(ApiResponse.Fail(
                "Hard delete requires the header 'X-Gdpr-Confirm: HARD-DELETE'. " +
                "Consider using /anonymize instead to preserve referential integrity."));

        var contact = await _db.Contacts
            .IgnoreQueryFilters()
            .Include(c => c.Documents)
            .Include(c => c.Interactions)
            .Include(c => c.Donations)
            .FirstOrDefaultAsync(c => c.Id == contactId, ct);

        if (contact is null)
            return NotFoundResponse($"Contact {contactId} not found.");

        if (contact.Donations.Any(d => !d.IsDeleted))
            return BadRequest(ApiResponse.Fail(
                "Cannot hard-delete a contact with confirmed donations. " +
                "Anonymize the contact instead to preserve the financial audit trail."));

        // Remove in dependency order
        _db.Documents.RemoveRange(contact.Documents);
        _db.Interactions.RemoveRange(contact.Interactions);
        _db.Contacts.Remove(contact);

        await _db.SaveChangesAsync(ct);
        return NoContentResponse();
    }
}
