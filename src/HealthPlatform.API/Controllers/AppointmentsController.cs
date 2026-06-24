using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Appointments;
using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/appointments")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class AppointmentsController(ISender sender) : ControllerBase
{
    [HttpPost("bookings")]
    [ProducesResponseType(typeof(BookAppointmentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<BookAppointmentDto>> BookAsync(
        [FromBody] BookAppointmentRequest request,
        CancellationToken ct)
    {
        var response = await sender.Send(BookAppointmentCommandMapper.ToCommand(request), ct);
        return Created($"/api/v1/appointments/bookings/{response.AppointmentId}", response);
    }
}
