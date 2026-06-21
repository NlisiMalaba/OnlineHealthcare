using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.UpdateDoctorProfile;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/identity/doctors/me")]
[Authorize(Policy = AuthorizationPolicies.Doctor)]
public sealed class DoctorProfileController(ISender sender) : ControllerBase
{
    [HttpPatch("profile")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DoctorProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DoctorProfileDto>> UpdateProfileAsync(
        [FromForm] UpdateDoctorProfileRequest request,
        CancellationToken ct)
    {
        var command = UpdateDoctorProfileCommandMapper.ToCommand(request);
        return Ok(await sender.Send(command, ct));
    }
}
