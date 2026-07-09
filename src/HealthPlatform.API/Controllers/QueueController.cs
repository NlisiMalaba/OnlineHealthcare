using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Queue;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Queue;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/queue")]
public sealed class QueueController(ISender sender) : ControllerBase
{
    [HttpPost("join")]
    [Authorize(Policy = AuthorizationPolicies.Patient)]
    [ProducesResponseType(typeof(QueueEntryDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<QueueEntryDto>> JoinAsync(
        [FromBody] JoinQueueRequest request,
        CancellationToken ct)
    {
        var entry = await sender.Send(QueueCommandMapper.ToJoinCommand(request), ct);
        return Created($"/api/v1/queue/entries/{entry.Id}", entry);
    }

    [HttpPost("advance")]
    [Authorize(Roles = $"{ApplicationRoles.Doctor},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(IReadOnlyList<QueueEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<QueueEntryDto>>> AdvanceAsync(CancellationToken ct) =>
        Ok(await sender.Send(QueueCommandMapper.ToAdvanceCommand(), ct));

    [HttpPost("entries/{queueEntryId:guid}/seen")]
    [Authorize(Roles = $"{ApplicationRoles.Doctor},{ApplicationRoles.Admin}")]
    [ProducesResponseType(typeof(QueueEntryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<QueueEntryDto>> MarkSeenAsync(Guid queueEntryId, CancellationToken ct) =>
        Ok(await sender.Send(QueueCommandMapper.ToMarkSeenCommand(queueEntryId), ct));

    [HttpPost("entries/{queueEntryId:guid}/absent")]
    [Authorize(Roles = $"{ApplicationRoles.Doctor},{ApplicationRoles.Admin}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAbsentAsync(Guid queueEntryId, CancellationToken ct)
    {
        await sender.Send(QueueCommandMapper.ToMarkAbsentCommand(queueEntryId), ct);
        return NoContent();
    }
}
