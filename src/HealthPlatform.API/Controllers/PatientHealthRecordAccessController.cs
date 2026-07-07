using HealthPlatform.API.Requests.HealthRecords;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.HealthRecords.GrantHealthRecordAccess;
using HealthPlatform.Application.HealthRecords.ListHealthRecordAccessGrants;
using HealthPlatform.Application.HealthRecords.RevokeHealthRecordAccess;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/patients/me/health-record/access")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class PatientHealthRecordAccessController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HealthRecordAccessDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HealthRecordAccessDto>>> ListAsync(CancellationToken ct) =>
        Ok(await sender.Send(new ListHealthRecordAccessGrantsQuery(), ct));

    [HttpPost]
    [ProducesResponseType(typeof(HealthRecordAccessDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<HealthRecordAccessDto>> GrantAsync(
        [FromBody] GrantHealthRecordAccessRequest request,
        CancellationToken ct)
    {
        var grant = await sender.Send(
            new GrantHealthRecordAccessCommand(request.DoctorId, request.AccessType, request.Sections),
            ct);

        return CreatedAtAction(nameof(ListAsync), grant);
    }

    [HttpDelete("{doctorId:guid}")]
    [ProducesResponseType(typeof(HealthRecordAccessDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthRecordAccessDto>> RevokeAsync(Guid doctorId, CancellationToken ct) =>
        Ok(await sender.Send(new RevokeHealthRecordAccessCommand(doctorId), ct));
}
