using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.HealthRecords.GetPatientHealthRecordEntry;
using HealthPlatform.Application.HealthRecords.ListPatientHealthRecordEntries;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/patients/me/health-record")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class PatientHealthRecordController(ISender sender) : ControllerBase
{
    [HttpGet("entries")]
    [ProducesResponseType(typeof(IReadOnlyList<HealthRecordEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HealthRecordEntryDto>>> ListEntriesAsync(CancellationToken ct) =>
        Ok(await sender.Send(new ListPatientHealthRecordEntriesQuery(), ct));

    [HttpGet("entries/{entryId}")]
    [ProducesResponseType(typeof(HealthRecordEntryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthRecordEntryDto>> GetEntryAsync(string entryId, CancellationToken ct) =>
        Ok(await sender.Send(new GetPatientHealthRecordEntryQuery(entryId), ct));
}
