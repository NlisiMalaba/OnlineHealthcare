using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Maternal;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Application.Maternal.BirthPlans;
using HealthPlatform.Application.Maternal.BirthPlans.GetBirthPlan;
using HealthPlatform.Application.Maternal.BirthPlans.ListMaternalCareAccessGrants;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/maternal/antenatal-records")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class AntenatalRecordsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(AntenatalRecordDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AntenatalRecordDto>> CreateAsync(
        [FromBody] CreateAntenatalRecordRequest request,
        CancellationToken ct)
    {
        var record = await sender.Send(MaternalCommandMapper.ToCreateAntenatalRecordCommand(request), ct);
        return Created($"/api/v1/maternal/antenatal-records/{record.Id}", record);
    }

    [HttpPost("{antenatalRecordId:guid}/birth-plan")]
    [ProducesResponseType(typeof(BirthPlanDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<BirthPlanDto>> CreateBirthPlanAsync(
        Guid antenatalRecordId,
        [FromBody] BirthPlanContentRequest request,
        CancellationToken ct)
    {
        var birthPlan = await sender.Send(
            MaternalCommandMapper.ToCreateBirthPlanCommand(antenatalRecordId, request),
            ct);
        return Created($"/api/v1/maternal/antenatal-records/{antenatalRecordId}/birth-plan", birthPlan);
    }

    [HttpPut("{antenatalRecordId:guid}/birth-plan")]
    [ProducesResponseType(typeof(BirthPlanDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BirthPlanDto>> UpdateBirthPlanAsync(
        Guid antenatalRecordId,
        [FromBody] BirthPlanContentRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(
            MaternalCommandMapper.ToUpdateBirthPlanCommand(antenatalRecordId, request),
            ct));

    [HttpGet("{antenatalRecordId:guid}/birth-plan")]
    [ProducesResponseType(typeof(BirthPlanDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BirthPlanDto>> GetBirthPlanAsync(
        Guid antenatalRecordId,
        CancellationToken ct) =>
        Ok(await sender.Send(new GetBirthPlanQuery(antenatalRecordId), ct));

    [HttpPost("{antenatalRecordId:guid}/access")]
    [ProducesResponseType(typeof(MaternalCareAccessGrantDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<MaternalCareAccessGrantDto>> GrantAccessAsync(
        Guid antenatalRecordId,
        [FromBody] GrantMaternalCareAccessRequest request,
        CancellationToken ct)
    {
        var grant = await sender.Send(
            MaternalCommandMapper.ToGrantMaternalCareAccessCommand(antenatalRecordId, request),
            ct);
        return Created($"/api/v1/maternal/antenatal-records/{antenatalRecordId}/access/{grant.DoctorId}", grant);
    }

    [HttpDelete("{antenatalRecordId:guid}/access/{doctorId:guid}")]
    [ProducesResponseType(typeof(MaternalCareAccessGrantDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MaternalCareAccessGrantDto>> RevokeAccessAsync(
        Guid antenatalRecordId,
        Guid doctorId,
        CancellationToken ct) =>
        Ok(await sender.Send(
            MaternalCommandMapper.ToRevokeMaternalCareAccessCommand(antenatalRecordId, doctorId),
            ct));

    [HttpGet("{antenatalRecordId:guid}/access")]
    [ProducesResponseType(typeof(IReadOnlyList<MaternalCareAccessGrantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MaternalCareAccessGrantDto>>> ListAccessGrantsAsync(
        Guid antenatalRecordId,
        CancellationToken ct) =>
        Ok(await sender.Send(new ListMaternalCareAccessGrantsQuery(antenatalRecordId), ct));
}
