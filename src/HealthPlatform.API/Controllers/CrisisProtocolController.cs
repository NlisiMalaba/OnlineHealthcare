using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.MentalHealth;
using HealthPlatform.Application.MentalHealth.CrisisProtocol;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/mental-health/crisis-protocol")]
public sealed class CrisisProtocolController(ISender sender) : ControllerBase
{
    [HttpPost("evaluate")]
    [Authorize(Policy = AuthorizationPolicies.Patient)]
    [ProducesResponseType(typeof(CrisisProtocolDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CrisisProtocolDto>> EvaluateAsync(
        [FromBody] EvaluateCrisisInputRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(MentalHealthCommandMapper.ToEvaluateCrisisInputCommand(request), ct));
}
