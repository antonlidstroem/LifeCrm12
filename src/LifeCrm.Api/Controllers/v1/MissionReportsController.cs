// src/LifeCrm.Api/Controllers/v1/MissionReportsController.cs
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Reports.Commands;
using LifeCrm.Application.Reports.DTOs;
using LifeCrm.Application.Reports.Queries;
using LifeCrm.Core.Constants;
using LifeCrm.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeCrm.Api.Controllers.v1;

public class MissionReportsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationParams paging,
        [FromQuery] Guid? campaignId,
        [FromQuery] Guid? projectId,
        [FromQuery] ReportStatus? status,
        CancellationToken ct)
        => OkResponse(await Mediator.Send(
            new GetReportsQuery(paging, campaignId, projectId, status), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetReportByIdQuery(id), ct));

    [HttpPost]
    [Authorize(Policy = "CanWrite")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateReportRequest request, CancellationToken ct)
    {
        var id = await Mediator.Send(new CreateReportCommand(request), ct);
        return CreatedResponse(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateReportRequest request, CancellationToken ct)
    {
        await Mediator.Send(new UpdateReportCommand(request with { Id = id }), ct);
        return NoContentResponse();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var isAdmin = User.IsInRole(Roles.Admin);
        await Mediator.Send(new DeleteReportCommand(id, isAdmin), ct);
        return NoContentResponse();
    }

    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new SubmitReportCommand(id), ct);
        return NoContentResponse();
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "FinanceOrAdmin")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var approverName = User.FindFirst("name")?.Value ?? "Admin";
        await Mediator.Send(new ApproveReportCommand(id, approverName), ct);
        return NoContentResponse();
    }

    [HttpPost("{id:guid}/return")]
    [Authorize(Policy = "FinanceOrAdmin")]
    public async Task<IActionResult> Return(
        Guid id, [FromBody] ReturnForRevisionRequest request, CancellationToken ct)
    {
        await Mediator.Send(new ReturnReportCommand(id, request), ct);
        return NoContentResponse();
    }

    [HttpPut("{id:guid}/decisions")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> UpsertDecision(
        Guid id, [FromBody] UpsertDecisionCountRequest request, CancellationToken ct)
        => OkResponse(await Mediator.Send(new UpsertDecisionCountCommand(id, request), ct));

    [HttpPost("{id:guid}/people-groups")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> AddPeopleGroup(
        Guid id, [FromBody] AddPeopleGroupRequest request, CancellationToken ct)
        => OkResponse(await Mediator.Send(new AddPeopleGroupCommand(id, request), ct));

    [HttpDelete("{id:guid}/people-groups/{entryId:guid}")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> RemovePeopleGroup(
        Guid id, Guid entryId, CancellationToken ct)
    {
        await Mediator.Send(new RemovePeopleGroupCommand(id, entryId), ct);
        return NoContentResponse();
    }

    [HttpGet("people-groups/search")]
    public async Task<IActionResult> SearchPeopleGroups(
        [FromQuery] string q, CancellationToken ct)
        => OkResponse(await Mediator.Send(new SearchPeopleGroupsQuery(q ?? ""), ct));

    [HttpPost("{id:guid}/prayer-points")]
    [Authorize(Policy = "CanWrite")]
    public async Task<IActionResult> AddPrayerPoint(
        Guid id, [FromBody] CreatePrayerPointRequest request, CancellationToken ct)
        => OkResponse(await Mediator.Send(
            new CreatePrayerPointCommand(request with { ReportId = id }), ct));

    [HttpGet("answered-prayers/this-month")]
    public async Task<IActionResult> AnsweredPrayersThisMonth(CancellationToken ct)
        => OkResponse(await Mediator.Send(new GetAnsweredPrayersThisMonthQuery(), ct));
}