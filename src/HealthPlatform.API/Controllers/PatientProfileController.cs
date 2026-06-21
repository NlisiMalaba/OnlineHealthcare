using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity.UpdatePatientProfile;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/identity/patients/me")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class PatientProfileController(ISender sender) : ControllerBase
{
    [HttpPatch("profile")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PatientProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PatientProfileDto>> UpdateProfileAsync(
        [FromForm] UpdatePatientProfileRequest request,
        CancellationToken ct)
    {
        var command = UpdatePatientProfileCommandMapper.ToCommand(request);
        return Ok(await sender.Send(command, ct));
    }
}
