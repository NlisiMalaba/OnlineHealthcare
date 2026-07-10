using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.MentalHealth;
using HealthPlatform.Application.MentalHealth.MoodLogs;
using HealthPlatform.Application.MentalHealth.MoodLogs.DeleteMoodLog;
using HealthPlatform.Application.MentalHealth.MoodLogs.GetMoodChartData;
using HealthPlatform.Application.MentalHealth.MoodLogs.GetMoodLog;
using HealthPlatform.Application.MentalHealth.MoodLogs.GetPatientMoodChartData;
using HealthPlatform.Application.MentalHealth.MoodLogs.GrantMoodChartSharingConsent;
using HealthPlatform.Application.MentalHealth.MoodLogs.ListMoodLogs;
using HealthPlatform.Application.MentalHealth.MoodLogs.RevokeMoodChartSharingConsent;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/mental-health/mood-logs")]
public sealed class MoodLogsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.Patient)]
    [ProducesResponseType(typeof(MoodLogDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<MoodLogDto>> CreateAsync(
        [FromBody] CreateMoodLogRequest request,
        CancellationToken ct)
    {
        var moodLog = await sender.Send(MentalHealthCommandMapper.ToCreateMoodLogCommand(request), ct);
        return Created($"/api/v1/mental-health/mood-logs/{moodLog.Id}", moodLog);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.Patient)]
    [ProducesResponseType(typeof(IReadOnlyList<MoodLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MoodLogDto>>> ListAsync(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken ct) =>
        Ok(await sender.Send(new ListMoodLogsQuery(fromUtc, toUtc), ct));

    [HttpGet("chart")]
    [Authorize(Policy = AuthorizationPolicies.Patient)]
    [ProducesResponseType(typeof(MoodChartDataDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MoodChartDataDto>> GetChartAsync(
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken ct) =>
        Ok(await sender.Send(new GetMoodChartDataQuery(fromUtc, toUtc), ct));

    [HttpGet("{moodLogId}")]
    [Authorize(Policy = AuthorizationPolicies.Patient)]
    [ProducesResponseType(typeof(MoodLogDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MoodLogDto>> GetAsync(string moodLogId, CancellationToken ct) =>
        Ok(await sender.Send(new GetMoodLogQuery(moodLogId), ct));

    [HttpPut("{moodLogId}")]
    [Authorize(Policy = AuthorizationPolicies.Patient)]
    [ProducesResponseType(typeof(MoodLogDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MoodLogDto>> UpdateAsync(
        string moodLogId,
        [FromBody] UpdateMoodLogRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(MentalHealthCommandMapper.ToUpdateMoodLogCommand(moodLogId, request), ct));

    [HttpDelete("{moodLogId}")]
    [Authorize(Policy = AuthorizationPolicies.Patient)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(string moodLogId, CancellationToken ct)
    {
        await sender.Send(new DeleteMoodLogCommand(moodLogId), ct);
        return NoContent();
    }

    [HttpPost("chart-sharing-consent")]
    [Authorize(Policy = AuthorizationPolicies.Patient)]
    [ProducesResponseType(typeof(MoodChartSharingConsentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MoodChartSharingConsentDto>> GrantChartSharingConsentAsync(
        [FromBody] GrantMoodChartSharingConsentRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(MentalHealthCommandMapper.ToGrantMoodChartConsentCommand(request), ct));

    [HttpDelete("chart-sharing-consent/{therapistId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Patient)]
    [ProducesResponseType(typeof(MoodChartSharingConsentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MoodChartSharingConsentDto>> RevokeChartSharingConsentAsync(
        Guid therapistId,
        CancellationToken ct) =>
        Ok(await sender.Send(MentalHealthCommandMapper.ToRevokeMoodChartConsentCommand(therapistId), ct));
}
