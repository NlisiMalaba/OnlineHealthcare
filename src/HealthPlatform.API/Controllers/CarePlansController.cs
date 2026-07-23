using HealthPlatform.Application.Security;
using HealthPlatform.Application.Wellness.CarePlans;
using HealthPlatform.Application.Wellness.CarePlans.CompleteCarePlanTask;
using HealthPlatform.Application.Wellness.CarePlans.GetCarePlan;
using HealthPlatform.Application.Wellness.CarePlans.ListCarePlans;
using HealthPlatform.Domain.Wellness;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/wellness/care-plans")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class CarePlansController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CarePlanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CarePlanDto>>> ListAsync(
        [FromQuery] CarePlanStatus? status,
        CancellationToken ct) =>
        Ok(await sender.Send(new ListCarePlansQuery(status), ct));

    [HttpGet("{carePlanId:guid}")]
    [ProducesResponseType(typeof(CarePlanDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CarePlanDto>> GetAsync(Guid carePlanId, CancellationToken ct) =>
        Ok(await sender.Send(new GetCarePlanQuery(carePlanId), ct));

    [HttpPost("{carePlanId:guid}/tasks/{taskId:guid}/complete")]
    [ProducesResponseType(typeof(CarePlanDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CarePlanDto>> CompleteTaskAsync(
        Guid carePlanId,
        Guid taskId,
        CancellationToken ct) =>
        Ok(await sender.Send(new CompleteCarePlanTaskCommand(carePlanId, taskId), ct));
}
