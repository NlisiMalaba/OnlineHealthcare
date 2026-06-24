using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Prescriptions;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/prescriptions")]
[Authorize(Policy = AuthorizationPolicies.Doctor)]
public sealed class PrescriptionsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(PrescriptionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PrescriptionDto>> CreateAsync(
        [FromBody] CreatePrescriptionRequest request,
        CancellationToken ct)
    {
        var prescription = await sender.Send(PrescriptionCommandMapper.ToCreateCommand(request), ct);
        return Created($"/api/v1/prescriptions/{prescription.Id}", prescription);
    }
}
