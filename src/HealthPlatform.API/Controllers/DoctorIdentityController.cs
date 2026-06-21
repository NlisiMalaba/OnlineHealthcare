using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity.RegisterDoctor;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/identity/doctors")]
[AllowAnonymous]
public sealed class DoctorIdentityController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DoctorRegistrationResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<DoctorRegistrationResponseDto>> RegisterDoctorAsync(
        [FromForm] RegisterDoctorRequest request,
        CancellationToken ct)
    {
        var command = RegisterDoctorCommandMapper.ToCommand(request);
        var response = await sender.Send(command, ct);
        return Created($"/api/v1/identity/doctors/{response.DoctorId}", response);
    }
}
