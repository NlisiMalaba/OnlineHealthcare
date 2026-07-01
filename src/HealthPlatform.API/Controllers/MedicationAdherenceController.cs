using HealthPlatform.API.Requests.Wellness;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Application.Wellness.ConfirmMedicationDose;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/wellness")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class MedicationAdherenceController(ISender sender) : ControllerBase
{
    [HttpPost("schedules/{scheduleId:guid}/doses/confirm")]
    [ProducesResponseType(typeof(AdherenceEventDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AdherenceEventDto>> ConfirmDoseAsync(
        Guid scheduleId,
        [FromBody] ConfirmMedicationDoseRequest request,
        CancellationToken ct)
    {
        var adherenceEvent = await sender.Send(
            new ConfirmMedicationDoseCommand(scheduleId, request.ScheduledAtUtc),
            ct);

        return Created(
            $"/api/v1/wellness/schedules/{scheduleId}/doses/{adherenceEvent.Id}",
            adherenceEvent);
    }
}
