using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Maternal;
using HealthPlatform.Application.Maternal.AntenatalRecords;
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
}
