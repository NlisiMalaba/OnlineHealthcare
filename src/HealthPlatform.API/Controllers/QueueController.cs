using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Queue;
using HealthPlatform.Application.Queue;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/queue")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class QueueController(ISender sender) : ControllerBase
{
    [HttpPost("join")]
    [ProducesResponseType(typeof(QueueEntryDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<QueueEntryDto>> JoinAsync(
        [FromBody] JoinQueueRequest request,
        CancellationToken ct)
    {
        var entry = await sender.Send(QueueCommandMapper.ToJoinCommand(request), ct);
        return Created($"/api/v1/queue/entries/{entry.Id}", entry);
    }
}
