using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Wellness;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Wellness.CarePlans;
using HealthPlatform.Application.Wellness.CarePlans.AssignCarePlan;
using HealthPlatform.Application.Wellness.CarePlans.GetDoctorCarePlan;
using HealthPlatform.Application.Wellness.CarePlans.ListDoctorCarePlans;
using HealthPlatform.Application.Wellness.CarePlans.UpdateCarePlan;
using HealthPlatform.Domain.Wellness;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/wellness/doctors/me/care-plans")]
[Authorize(Policy = AuthorizationPolicies.Doctor)]
public sealed class DoctorCarePlansController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CarePlanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CarePlanDto>>> ListAsync(
        [FromQuery] Guid? patientId,
        [FromQuery] CarePlanStatus? status,
        CancellationToken ct) =>
        Ok(await sender.Send(new ListDoctorCarePlansQuery(patientId, status), ct));

    [HttpGet("{carePlanId:guid}")]
    [ProducesResponseType(typeof(CarePlanDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CarePlanDto>> GetAsync(Guid carePlanId, CancellationToken ct) =>
        Ok(await sender.Send(new GetDoctorCarePlanQuery(carePlanId), ct));

    [HttpPost]
    [ProducesResponseType(typeof(CarePlanDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CarePlanDto>> AssignAsync(
        [FromBody] AssignCarePlanRequest request,
        CancellationToken ct)
    {
        var plan = await sender.Send(WellnessCommandMapper.ToAssignCarePlanCommand(request), ct);
        return CreatedAtAction(nameof(GetAsync), new { carePlanId = plan.Id }, plan);
    }

    [HttpPut("{carePlanId:guid}")]
    [ProducesResponseType(typeof(CarePlanDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CarePlanDto>> UpdateAsync(
        Guid carePlanId,
        [FromBody] UpdateCarePlanRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(WellnessCommandMapper.ToUpdateCarePlanCommand(carePlanId, request), ct));
}
