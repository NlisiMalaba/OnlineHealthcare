using HealthPlatform.Application.MentalHealth.MoodLogs;
using HealthPlatform.Application.MentalHealth.MoodLogs.GetPatientMoodChartData;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/mental-health/patients/{patientId:guid}/mood-logs")]
[Authorize(Policy = AuthorizationPolicies.Doctor)]
public sealed class PatientMoodLogsController(ISender sender) : ControllerBase
{
    [HttpGet("chart")]
    [ProducesResponseType(typeof(MoodChartDataDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MoodChartDataDto>> GetChartAsync(
        Guid patientId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken ct) =>
        Ok(await sender.Send(new GetPatientMoodChartDataQuery(patientId, fromUtc, toUtc), ct));
}
