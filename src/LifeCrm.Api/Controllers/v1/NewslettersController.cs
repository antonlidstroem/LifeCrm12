using LifeCrm.Contracts.Common.DTOs;
using LifeCrm.Application.Newsletters.Commands;
using LifeCrm.Contracts.Newsletters.DTOs;
using LifeCrm.Application.Newsletters.Queries;
using LifeCrm.Core.Enums;
using LifeCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeCrm.Api.Controllers.v1;

public class NewslettersController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams paging, [FromQuery] NewsletterStatus? status, CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetNewslettersQuery(paging, status), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetNewsletterByIdQuery(id), ct));

    [HttpPost]
    [Authorize(Policy = "CanWrite")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateNewsletterRequest request, CancellationToken ct)
    {
        var id = await Mediator.Send(new CreateNewsletterCommand(request), ct);
        return CreatedResponse(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNewsletterRequest request, CancellationToken ct)
    {
        await Mediator.Send(new UpdateNewsletterCommand(request with { Id = id }), ct);
        return NoContentResponse();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteNewsletterCommand(id), ct);
        return NoContentResponse();
    }

    [HttpGet("recipients/preview")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> PreviewRecipients([FromQuery] string? tagFilter, [FromQuery] string? contactTypeFilter, CancellationToken ct)
        => OkResponse(await Mediator.Send(new PreviewNewsletterRecipientsCommand(tagFilter, contactTypeFilter), ct));

    [HttpPost("{id:guid}/send")]
    [Authorize(Policy = "FinanceOrAdmin")]
    public async Task<IActionResult> Send(Guid id, [FromBody] SendNewsletterRequest request, CancellationToken ct)
        => OkResponse(await Mediator.Send(new SendNewsletterCommand(id, request), ct));

    [HttpPost("{id:guid}/attachments")]
    [Authorize(Policy = "CanWrite")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return BadRequest(ApiResponse.Fail("No file received."));
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        var result = await Mediator.Send(new AddNewsletterAttachmentCommand(id, file.FileName, file.ContentType, ms.ToArray()), ct);
        return OkResponse(result);
    }

    [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> DeleteAttachment(Guid id, Guid attachmentId, CancellationToken ct)
    {
        await Mediator.Send(new DeleteNewsletterAttachmentCommand(id, attachmentId), ct);
        return NoContentResponse();
    }

    [HttpGet("{id:guid}/attachments/{attachmentId:guid}/download")]
    public async Task<IActionResult> DownloadAttachment(Guid id, Guid attachmentId, [FromServices] AppDbContext context, CancellationToken ct)
    {
        var att = await context.NewsletterAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId && a.NewsletterId == id, ct);
        if (att is null) return NotFound();
        return File(att.FileBytes, att.ContentType, att.FileName);
    }
}
