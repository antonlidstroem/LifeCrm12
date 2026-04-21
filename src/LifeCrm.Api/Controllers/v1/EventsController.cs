using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Events.Commands;
using LifeCrm.Application.Events.DTOs;
using LifeCrm.Application.Events.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeCrm.Api.Controllers.v1;

public class EventsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EventListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationParams paging,
        CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetEventsQuery(paging), ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetEventByIdQuery(id), ct));

    [HttpPost]
    [Authorize(Policy = "CanWrite")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEventRequest request, CancellationToken ct)
    {
        var id = await Mediator.Send(new CreateEventCommand(request), ct);
        return CreatedResponse(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateEventRequest request, CancellationToken ct)
    {
        await Mediator.Send(new UpdateEventCommand(request with { Id = id }), ct);
        return NoContentResponse();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteEventCommand(id), ct);
        return NoContentResponse();
    }

    /// <summary>
    /// Check-in a contact to an event. Accepts either an existing ContactId
    /// or a FirstName + Email for instant new-contact creation.
    /// This is the zero-friction engagement entry point.
    /// </summary>
    [HttpPost("{id:guid}/checkin")]
    [Authorize(Policy = "CanWrite")]
    [ProducesResponseType(typeof(ApiResponse<CheckInResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckIn(
        Guid id, [FromBody] CheckInRequest request, CancellationToken ct)
        => OkResponse(await Mediator.Send(new CheckInCommand(id, request), ct));

    [HttpDelete("{id:guid}/attendances/{attendanceId:guid}")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> RemoveAttendance(
        Guid id, Guid attendanceId, CancellationToken ct)
    {
        await Mediator.Send(new RemoveAttendanceCommand(attendanceId), ct);
        return NoContentResponse();
    }

    /// <summary>Returns the engagement summary for a specific contact.</summary>
    [HttpGet("contacts/{contactId:guid}/engagement")]
    [ProducesResponseType(typeof(ApiResponse<ContactEngagementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContactEngagement(
        Guid contactId, CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetContactEngagementQuery(contactId), ct));
}
