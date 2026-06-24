using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Appointments;
using HealthPlatform.Application.Appointments.RescheduleAppointment;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/appointments/doctors/me/bookings")]
[Authorize(Policy = AuthorizationPolicies.Doctor)]
public sealed class DoctorAppointmentsController(ISender sender) : ControllerBase
{
    [HttpPost("{appointmentId:guid}/reschedule")]
    [ProducesResponseType(typeof(RescheduleAppointmentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RescheduleAppointmentDto>> RescheduleAsync(
        Guid appointmentId,
        [FromBody] RescheduleAppointmentRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(
            RescheduleAppointmentCommandMapper.ToCommand(appointmentId, request),
            ct));
}
