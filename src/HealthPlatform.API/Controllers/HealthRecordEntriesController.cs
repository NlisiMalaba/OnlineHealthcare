using HealthPlatform.API.Mapping;
using ApiRequests = HealthPlatform.API.Requests.HealthRecords;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.HealthRecords.CreateHealthRecordEntry;
using HealthPlatform.Application.HealthRecords.DeleteHealthRecordEntry;
using HealthPlatform.Application.HealthRecords.GetHealthRecordEntry;
using HealthPlatform.Application.HealthRecords.ListHealthRecordEntries;
using HealthPlatform.Application.HealthRecords.UpdateHealthRecordEntry;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/health-records")]
[Authorize(Policy = AuthorizationPolicies.Doctor)]
public sealed class HealthRecordEntriesController(ISender sender) : ControllerBase
{
    [HttpGet("{healthRecordId:guid}/entries")]
    [ProducesResponseType(typeof(IReadOnlyList<HealthRecordEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HealthRecordEntryDto>>> ListAsync(
        Guid healthRecordId,
        CancellationToken ct) =>
        Ok(await sender.Send(new ListHealthRecordEntriesQuery(healthRecordId), ct));

    [HttpGet("entries/{entryId}")]
    [ProducesResponseType(typeof(HealthRecordEntryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthRecordEntryDto>> GetAsync(string entryId, CancellationToken ct) =>
        Ok(await sender.Send(new GetHealthRecordEntryQuery(entryId), ct));

    [HttpPost("{healthRecordId:guid}/entries")]
    [ProducesResponseType(typeof(HealthRecordEntryDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<HealthRecordEntryDto>> CreateAsync(
        Guid healthRecordId,
        [FromBody] ApiRequests.CreateHealthRecordEntryRequest request,
        CancellationToken ct)
    {
        var entry = await sender.Send(
            HealthRecordEntryCommandMapper.ToCreateCommand(healthRecordId, request),
            ct);

        return CreatedAtAction(
            nameof(GetAsync),
            new { entryId = entry.Id },
            entry);
    }

    [HttpPut("entries/{entryId}")]
    [ProducesResponseType(typeof(HealthRecordEntryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthRecordEntryDto>> UpdateAsync(
        string entryId,
        [FromBody] ApiRequests.UpdateHealthRecordEntryRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(HealthRecordEntryCommandMapper.ToUpdateCommand(entryId, request), ct));

    [HttpDelete("entries/{entryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(string entryId, CancellationToken ct)
    {
        await sender.Send(new DeleteHealthRecordEntryCommand(entryId), ct);
        return NoContent();
    }
}
