using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Maternal;
using HealthPlatform.Application.Maternal.ChildProfiles;
using HealthPlatform.Application.Vaccinations;
using HealthPlatform.Application.Maternal.ChildProfiles.CreateChildProfile;
using HealthPlatform.Application.Maternal.ChildProfiles.GetChildProfile;
using HealthPlatform.Application.Maternal.ChildProfiles.ListChildProfiles;
using HealthPlatform.Application.Vaccinations.RecordChildVaccination;
using HealthPlatform.Application.Maternal.GrowthEntries;
using HealthPlatform.Application.Maternal.GrowthEntries.GetChildGrowthChart;
using HealthPlatform.Application.Maternal.GrowthEntries.ListGrowthEntries;
using HealthPlatform.Application.Vaccinations.ListChildVaccinationRecords;
using HealthPlatform.Application.Vaccinations.ListChildVaccinationSchedule;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/maternal/child-profiles")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class ChildProfilesController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ChildProfileDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ChildProfileDto>> CreateAsync(
        [FromBody] CreateChildProfileRequest request,
        CancellationToken ct)
    {
        var profile = await sender.Send(MaternalCommandMapper.ToCreateChildProfileCommand(request), ct);
        return Created($"/api/v1/maternal/child-profiles/{profile.Id}", profile);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ChildProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ChildProfileDto>>> ListAsync(CancellationToken ct)
    {
        var profiles = await sender.Send(new ListChildProfilesQuery(), ct);
        return Ok(profiles);
    }

    [HttpGet("{childProfileId:guid}")]
    [ProducesResponseType(typeof(ChildProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChildProfileDto>> GetAsync(Guid childProfileId, CancellationToken ct)
    {
        var profile = await sender.Send(new GetChildProfileQuery(childProfileId), ct);
        return Ok(profile);
    }

    [HttpGet("{childProfileId:guid}/vaccination-schedule")]
    [ProducesResponseType(typeof(IReadOnlyList<VaccinationScheduleEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VaccinationScheduleEntryDto>>> ListVaccinationScheduleAsync(
        Guid childProfileId,
        CancellationToken ct)
    {
        var schedule = await sender.Send(new ListChildVaccinationScheduleQuery(childProfileId), ct);
        return Ok(schedule);
    }

    [HttpGet("{childProfileId:guid}/vaccination-records")]
    [ProducesResponseType(typeof(IReadOnlyList<VaccinationRecordDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VaccinationRecordDto>>> ListVaccinationRecordsAsync(
        Guid childProfileId,
        CancellationToken ct)
    {
        var records = await sender.Send(new ListChildVaccinationRecordsQuery(childProfileId), ct);
        return Ok(records);
    }

    [HttpPost("{childProfileId:guid}/vaccinations")]
    [ProducesResponseType(typeof(VaccinationRecordDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<VaccinationRecordDto>> RecordVaccinationAsync(
        Guid childProfileId,
        [FromBody] RecordVaccinationRequest request,
        CancellationToken ct)
    {
        var record = await sender.Send(
            MaternalCommandMapper.ToRecordChildVaccinationCommand(childProfileId, request),
            ct);
        return Created($"/api/v1/maternal/child-profiles/{childProfileId}/vaccination-records/{record.Id}", record);
    }

    [HttpPost("{childProfileId:guid}/growth-entries")]
    [ProducesResponseType(typeof(GrowthEntryDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<GrowthEntryDto>> RecordGrowthEntryAsync(
        Guid childProfileId,
        [FromBody] RecordGrowthEntryRequest request,
        CancellationToken ct)
    {
        var entry = await sender.Send(
            MaternalCommandMapper.ToRecordGrowthEntryCommand(childProfileId, request),
            ct);
        return Created($"/api/v1/maternal/child-profiles/{childProfileId}/growth-entries/{entry.Id}", entry);
    }

    [HttpGet("{childProfileId:guid}/growth-entries")]
    [ProducesResponseType(typeof(IReadOnlyList<GrowthEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GrowthEntryDto>>> ListGrowthEntriesAsync(
        Guid childProfileId,
        CancellationToken ct)
    {
        var entries = await sender.Send(new ListGrowthEntriesQuery(childProfileId), ct);
        return Ok(entries);
    }

    [HttpGet("{childProfileId:guid}/growth-chart")]
    [ProducesResponseType(typeof(GrowthChartDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<GrowthChartDto>> GetGrowthChartAsync(
        Guid childProfileId,
        CancellationToken ct)
    {
        var chart = await sender.Send(new GetChildGrowthChartQuery(childProfileId), ct);
        return Ok(chart);
    }
}
