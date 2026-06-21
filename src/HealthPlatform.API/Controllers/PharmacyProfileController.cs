using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.UpdatePharmacyProfile;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/identity/pharmacies/me")]
[Authorize(Policy = AuthorizationPolicies.Pharmacy)]
public sealed class PharmacyProfileController(ISender sender) : ControllerBase
{
    [HttpPatch("profile")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PharmacyProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PharmacyProfileDto>> UpdateProfileAsync(
        [FromForm] UpdatePharmacyProfileRequest request,
        CancellationToken ct)
    {
        var command = UpdatePharmacyProfileCommandMapper.ToCommand(request);
        return Ok(await sender.Send(command, ct));
    }
}
