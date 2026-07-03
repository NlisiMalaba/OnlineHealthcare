using HealthPlatform.API.Requests.NextOfKin;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.NextOfKin.SendDoctorEmergencyAlert;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/next-of-kin/emergency-alerts")]
[Authorize(Policy = AuthorizationPolicies.Doctor)]
public sealed class DoctorEmergencyAlertsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(EmergencyAlertDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<EmergencyAlertDto>> SendAsync(
        [FromBody] SendDoctorEmergencyAlertRequest request,
        CancellationToken ct)
    {
        var alert = await sender.Send(
            new SendDoctorEmergencyAlertCommand(
                request.PatientId,
                request.AppointmentId,
                request.TriggerReason),
            ct);

        return Created($"/api/v1/next-of-kin/emergency-alerts/{alert.Id}", alert);
    }
}
