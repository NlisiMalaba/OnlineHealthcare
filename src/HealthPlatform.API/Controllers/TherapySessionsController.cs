using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.MentalHealth;
using HealthPlatform.Application.MentalHealth;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/mental-health/therapy-sessions")]
public sealed class TherapySessionsController(ISender sender) : ControllerBase
{
    [HttpPost("{therapySessionId:guid}/complete")]
    [Authorize(Policy = AuthorizationPolicies.Doctor)]
    [ProducesResponseType(typeof(TherapySessionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TherapySessionDto>> CompleteAsync(
        Guid therapySessionId,
        [FromBody] CompleteTherapySessionRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(MentalHealthCommandMapper.ToCompleteCommand(therapySessionId, request), ct));

    [HttpPost("{therapySessionId:guid}/grant-broader-access")]
    [Authorize(Policy = AuthorizationPolicies.Patient)]
    [ProducesResponseType(typeof(TherapySessionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TherapySessionDto>> GrantBroaderAccessAsync(
        Guid therapySessionId,
        CancellationToken ct) =>
        Ok(await sender.Send(MentalHealthCommandMapper.ToGrantBroaderAccessCommand(therapySessionId), ct));
}
