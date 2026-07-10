using HealthPlatform.API.Requests.Wellness;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Vaccinations;
using HealthPlatform.Application.Vaccinations.ListPatientVaccinationRecords;
using HealthPlatform.Application.Vaccinations.ListPatientVaccinationSchedule;
using HealthPlatform.Application.Vaccinations.RecordPatientVaccination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/wellness/vaccinations")]
public sealed class VaccinationsController(ISender sender) : ControllerBase
{
    [HttpGet("schedule")]
    [Authorize(Policy = AuthorizationPolicies.Patient)]
    [ProducesResponseType(typeof(IReadOnlyList<VaccinationScheduleEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VaccinationScheduleEntryDto>>> ListScheduleAsync(CancellationToken ct)
    {
        var schedule = await sender.Send(new ListPatientVaccinationScheduleQuery(), ct);
        return Ok(schedule);
    }

    [HttpGet("records")]
    [Authorize(Policy = AuthorizationPolicies.Patient)]
    [ProducesResponseType(typeof(IReadOnlyList<VaccinationRecordDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VaccinationRecordDto>>> ListRecordsAsync(CancellationToken ct)
    {
        var records = await sender.Send(new ListPatientVaccinationRecordsQuery(), ct);
        return Ok(records);
    }

    [HttpPost]
    [Authorize(Roles = $"{ApplicationRoles.Doctor},{ApplicationRoles.Patient}")]
    [ProducesResponseType(typeof(VaccinationRecordDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<VaccinationRecordDto>> RecordAsync(
        [FromBody] RecordPatientVaccinationRequest request,
        CancellationToken ct)
    {
        var record = await sender.Send(
            new RecordPatientVaccinationCommand(
                request.PatientId,
                request.ScheduleEntryId,
                request.VaccineName,
                request.AdministeredDate,
                request.BatchNumber,
                request.Provider),
            ct);

        return Created($"/api/v1/wellness/vaccinations/records/{record.Id}", record);
    }
}
