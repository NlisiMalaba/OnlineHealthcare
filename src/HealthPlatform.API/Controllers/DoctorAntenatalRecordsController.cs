using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Maternal;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Application.Maternal.BirthPlans;
using HealthPlatform.Application.Maternal.BirthPlans.GetBirthPlan;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/maternal/antenatal-records")]
[Authorize(Policy = AuthorizationPolicies.Doctor)]
public sealed class DoctorAntenatalRecordsController(ISender sender) : ControllerBase
{
    [HttpPost("{antenatalRecordId:guid}/checkups")]
    [ProducesResponseType(typeof(AntenatalCheckupEntryDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AntenatalCheckupEntryDto>> RecordCheckupAsync(
        Guid antenatalRecordId,
        [FromBody] RecordAntenatalCheckupRequest request,
        CancellationToken ct)
    {
        var entry = await sender.Send(
            MaternalCommandMapper.ToRecordAntenatalCheckupCommand(antenatalRecordId, request),
            ct);
        return Created(
            $"/api/v1/maternal/antenatal-records/{antenatalRecordId}/checkups/{entry.Id}",
            entry);
    }

    [HttpPut("{antenatalRecordId:guid}/fetal-monitoring-reminders")]
    [ProducesResponseType(typeof(AntenatalRecordDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AntenatalRecordDto>> ConfigureFetalMonitoringRemindersAsync(
        Guid antenatalRecordId,
        [FromBody] ConfigureFetalMonitoringRemindersRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(
            MaternalCommandMapper.ToConfigureFetalMonitoringRemindersCommand(antenatalRecordId, request),
            ct));

    [HttpGet("{antenatalRecordId:guid}/birth-plan")]
    [ProducesResponseType(typeof(BirthPlanDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BirthPlanDto>> GetBirthPlanAsync(
        Guid antenatalRecordId,
        CancellationToken ct) =>
        Ok(await sender.Send(new GetBirthPlanQuery(antenatalRecordId), ct));
}
