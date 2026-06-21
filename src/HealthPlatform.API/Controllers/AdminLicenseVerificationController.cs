using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.RejectDoctorLicense;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/admin/license-verifications")]
[Authorize(Policy = AuthorizationPolicies.Admin)]
public sealed class AdminLicenseVerificationController(ISender sender) : ControllerBase
{
    [HttpPost("{doctorId:guid}/verify")]
    [ProducesResponseType(typeof(LicenseVerificationResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LicenseVerificationResultDto>> VerifyDoctorLicenseAsync(
        Guid doctorId,
        CancellationToken ct)
    {
        var result = await sender.Send(new VerifyDoctorLicenseCommand(doctorId), ct);
        return Ok(result);
    }

    [HttpPost("{doctorId:guid}/reject")]
    [ProducesResponseType(typeof(LicenseVerificationResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LicenseVerificationResultDto>> RejectDoctorLicenseAsync(
        Guid doctorId,
        [FromBody] RejectDoctorLicenseRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new RejectDoctorLicenseCommand(doctorId, request.Reason),
            ct);
        return Ok(result);
    }
}
