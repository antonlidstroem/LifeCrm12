using LifeCrm.Application.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LifeCrm.Api.Controllers.v1;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    protected IActionResult OkResponse<T>(T data) =>
        Ok(ApiResponse<T>.Ok(data));

    protected IActionResult CreatedResponse<T>(string routeName, object routeValues, T data) =>
        CreatedAtRoute(routeName, routeValues, ApiResponse<T>.Ok(data));

    protected IActionResult NoContentResponse() => NoContent();

    protected IActionResult NotFoundResponse(string message) =>
        NotFound(ApiResponse.Fail(message));

    protected IActionResult BadRequest(ApiResponse response) =>
        base.BadRequest(response);
}
