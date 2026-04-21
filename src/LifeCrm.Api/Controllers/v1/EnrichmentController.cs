using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Enrichment.Commands;
using LifeCrm.Application.Enrichment.DTOs;
using LifeCrm.Application.Enrichment.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeCrm.Api.Controllers.v1;

/// <summary>
/// Admin-only enrichment endpoints: tags, contact profiles, mentoring relationships.
///
/// VISIBILITY RULE: No data from this controller must ever appear in public-facing
/// responses or non-admin API endpoints. The [AdminOnly] attribute on entities serves
/// as a code-review gate; this controller's [Authorize(Policy="AdminOnly")] is the
/// runtime gate.
///
/// Exception: tag assignment (POST/DELETE contact tags) is allowed for CanWrite
/// because tags are less sensitive than profile/mentoring data.
/// </summary>
[Route("api/v1/enrichment")]
[ApiController]
[Authorize]
public class EnrichmentController : ApiControllerBase
{
    // ── Tags (admin manages taxonomy; CanWrite can assign) ────────────────

    [HttpGet("tags")]
    [Authorize(Policy = "CanWrite")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TagDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTags(CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetTagsQuery(), ct));

    [HttpPost("tags")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<TagDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTag(
        [FromBody] CreateTagRequest request, CancellationToken ct)
    {
        var tag = await Mediator.Send(new CreateTagCommand(request), ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TagDto>.Ok(tag));
    }

    [HttpDelete("tags/{tagId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteTag(Guid tagId, CancellationToken ct)
    {
        await Mediator.Send(new DeleteTagCommand(tagId), ct);
        return NoContentResponse();
    }

    [HttpGet("contacts/{contactId:guid}/tags")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> GetContactTags(Guid contactId, CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetContactTagsQuery(contactId), ct));

    [HttpPost("contacts/{contactId:guid}/tags")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> AssignTag(
        Guid contactId, [FromBody] AssignTagRequest request, CancellationToken ct)
    {
        await Mediator.Send(new AssignTagCommand(contactId, request.TagId), ct);
        return NoContentResponse();
    }

    [HttpDelete("contacts/{contactId:guid}/tags/{tagId:guid}")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> RemoveTag(
        Guid contactId, Guid tagId, CancellationToken ct)
    {
        await Mediator.Send(new RemoveTagCommand(contactId, tagId), ct);
        return NoContentResponse();
    }

    // ── ContactProfile (admin only) ───────────────────────────────────────

    [HttpGet("contacts/{contactId:guid}/profile")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<ContactProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile(Guid contactId, CancellationToken ct)
    {
        var profile = await Mediator.Send(new GetContactProfileQuery(contactId), ct);
        return profile is null ? NotFoundResponse("No profile exists for this contact.") : OkResponse(profile);
    }

    [HttpPut("contacts/{contactId:guid}/profile")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<ContactProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertProfile(
        Guid contactId, [FromBody] UpsertContactProfileRequest request, CancellationToken ct)
        => OkResponse(await Mediator.Send(new UpsertContactProfileCommand(contactId, request), ct));

    [HttpDelete("contacts/{contactId:guid}/profile")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteProfile(Guid contactId, CancellationToken ct)
    {
        await Mediator.Send(new DeleteContactProfileCommand(contactId), ct);
        return NoContentResponse();
    }

    // ── Admin full contact view ───────────────────────────────────────────

    [HttpGet("contacts/{contactId:guid}/full")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<AdminContactDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminContactDetail(Guid contactId, CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetAdminContactDetailQuery(contactId), ct));

    // ── Mentoring (admin only) ────────────────────────────────────────────

    [HttpGet("contacts/{contactId:guid}/mentoring")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetMentoring(Guid contactId, CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetMentoringByContactQuery(contactId), ct));

    [HttpPost("mentoring")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<MentorRelationshipDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateMentoring(
        [FromBody] CreateMentorRelationshipRequest request, CancellationToken ct)
    {
        var rel = await Mediator.Send(new CreateMentorRelationshipCommand(request), ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<MentorRelationshipDto>.Ok(rel));
    }

    [HttpDelete("mentoring/{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteMentoring(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteMentorRelationshipCommand(id), ct);
        return NoContentResponse();
    }
}
