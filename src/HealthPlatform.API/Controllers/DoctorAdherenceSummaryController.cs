using HealthPlatform.Application.Security;
using HealthPlatform.Application.Wellness.Summaries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/wellness/doctors/me")]
[Authorize(Policy = AuthorizationPolicies.Doctor)]
public sealed class DoctorAdherenceSummaryController(ISender sender) : ControllerBase
{
    [HttpGet("patients/{patientId:guid}/adherence/summary")]
    [ProducesResponseType(typeof(AdherenceSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdherenceSummaryDto>> GetPatientAdherenceSummaryAsync(
        Guid patientId,
        [FromQuery] AdherenceSummaryPeriod period,
        CancellationToken ct) =>
        Ok(await sender.Send(new GetDoctorPatientAdherenceSummaryQuery(patientId, period), ct));
}
