using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Wellness;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Wellness.WellnessEntries;
using HealthPlatform.Application.Wellness.WellnessEntries.GetWellnessMetricChart;
using HealthPlatform.Application.Wellness.WellnessEntries.ListWellnessEntries;
using HealthPlatform.Domain.Wellness;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/wellness/entries")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class WellnessEntriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WellnessEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WellnessEntryDto>>> ListAsync(
        [FromQuery] WellnessMetricType? metricType,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken ct) =>
        Ok(await sender.Send(new ListWellnessEntriesQuery(metricType, fromUtc, toUtc), ct));

    [HttpPost]
    [ProducesResponseType(typeof(WellnessEntryDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<WellnessEntryDto>> RecordAsync(
        [FromBody] RecordWellnessEntryRequest request,
        CancellationToken ct)
    {
        var entry = await sender.Send(WellnessCommandMapper.ToRecordEntryCommand(request), ct);
        return Created($"/api/v1/wellness/entries/{entry.Id}", entry);
    }

    [HttpGet("chart")]
    [ProducesResponseType(typeof(WellnessMetricChartDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<WellnessMetricChartDto>> GetChartAsync(
        [FromQuery] WellnessMetricType metricType,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken ct) =>
        Ok(await sender.Send(new GetWellnessMetricChartQuery(metricType, fromUtc, toUtc), ct));
}
