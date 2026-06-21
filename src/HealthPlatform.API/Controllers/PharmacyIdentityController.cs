using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity.RegisterPharmacy;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/identity/pharmacies")]
[AllowAnonymous]
public sealed class PharmacyIdentityController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PharmacyRegistrationResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<PharmacyRegistrationResponseDto>> RegisterPharmacyAsync(
        [FromForm] RegisterPharmacyRequest request,
        CancellationToken ct)
    {
        var command = RegisterPharmacyCommandMapper.ToCommand(request);
        var response = await sender.Send(command, ct);
        return Created($"/api/v1/identity/pharmacies/{response.PharmacyId}", response);
    }
}
