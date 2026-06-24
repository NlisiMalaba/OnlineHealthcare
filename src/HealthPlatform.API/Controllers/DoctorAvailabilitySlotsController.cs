using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Appointments;
using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Appointments.AvailabilitySlots;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/appointments/doctors/me/availability-slots")]
[Authorize(Policy = AuthorizationPolicies.Doctor)]
public sealed class DoctorAvailabilitySlotsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DoctorAvailabilitySlotDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DoctorAvailabilitySlotDto>>> ListAsync(CancellationToken ct) =>
        Ok(await sender.Send(new ListDoctorAvailabilitySlotsQuery(), ct));

    [HttpGet("{slotId:guid}")]
    [ProducesResponseType(typeof(DoctorAvailabilitySlotDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DoctorAvailabilitySlotDto>> GetAsync(Guid slotId, CancellationToken ct) =>
        Ok(await sender.Send(new GetDoctorAvailabilitySlotQuery(slotId), ct));

    [HttpPost]
    [ProducesResponseType(typeof(DoctorAvailabilitySlotDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<DoctorAvailabilitySlotDto>> CreateAsync(
        [FromBody] DoctorAvailabilitySlotUpsertRequest request,
        CancellationToken ct)
    {
        var slot = await sender.Send(DoctorAvailabilitySlotCommandMapper.ToCreateCommand(request), ct);
        return CreatedAtAction(nameof(GetAsync), new { slotId = slot.Id }, slot);
    }

    [HttpPut("{slotId:guid}")]
    [ProducesResponseType(typeof(DoctorAvailabilitySlotDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DoctorAvailabilitySlotDto>> UpdateAsync(
        Guid slotId,
        [FromBody] DoctorAvailabilitySlotUpsertRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(DoctorAvailabilitySlotCommandMapper.ToUpdateCommand(slotId, request), ct));

    [HttpDelete("{slotId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(Guid slotId, CancellationToken ct)
    {
        await sender.Send(new DeleteDoctorAvailabilitySlotCommand(slotId), ct);
        return NoContent();
    }
}
