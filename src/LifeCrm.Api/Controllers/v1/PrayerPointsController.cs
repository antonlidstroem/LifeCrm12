// src/LifeCrm.Api/Controllers/v1/PrayerPointsController.cs
using LifeCrm.Contracts.Common.DTOs;
using LifeCrm.Application.Reports.Commands;
using LifeCrm.Contracts.Reports.DTOs;
using LifeCrm.Application.Reports.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeCrm.Api.Controllers.v1;

public class PrayerPointsController : ApiControllerBase
{
    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetActivePrayerPointsQuery(), ct));

    [HttpPost]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> Create(
        [FromBody] CreatePrayerPointRequest request, CancellationToken ct)
        => OkResponse(await Mediator.Send(new CreatePrayerPointCommand(request), ct));

    [HttpPost("{id:guid}/mark-answered")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> MarkAnswered(
        Guid id, [FromBody] MarkAnsweredRequest request, CancellationToken ct)
        => OkResponse(await Mediator.Send(new MarkPrayerAnsweredCommand(id, request), ct));

    // FIX: Admin can delete any prayer point; CanWrite users can delete their own
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeletePrayerPointCommand(id), ct);
        return NoContentResponse();
    }
}