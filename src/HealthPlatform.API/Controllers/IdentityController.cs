using HealthPlatform.Application.Identity.RegisterPatient;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/identity/patients")]
[AllowAnonymous]
public sealed class IdentityController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(PatientRegistrationResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PatientRegistrationResponseDto>> RegisterPatientAsync(
        [FromBody] RegisterPatientCommand command,
        CancellationToken ct)
    {
        var response = await sender.Send(command, ct);
        return Created($"/api/v1/identity/patients/{response.PatientId}", response);
    }
}
