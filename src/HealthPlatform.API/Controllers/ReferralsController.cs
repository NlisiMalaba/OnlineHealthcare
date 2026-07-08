using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Referrals;
using HealthPlatform.Application.Referrals;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/referrals")]
[Authorize(Policy = AuthorizationPolicies.Doctor)]
public sealed class ReferralsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ReferralDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReferralDto>> CreateAsync(
        [FromBody] CreateReferralRequest request,
        CancellationToken ct)
    {
        var referral = await sender.Send(ReferralCommandMapper.ToCreateCommand(request), ct);
        return Created($"/api/v1/referrals/{referral.Id}", referral);
    }

    [HttpPost("{referralId:guid}/respond")]
    [ProducesResponseType(typeof(ReferralDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReferralDto>> RespondAsync(
        Guid referralId,
        [FromBody] RespondToReferralRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(ReferralCommandMapper.ToRespondCommand(referralId, request), ct));

    [HttpPost("{referralId:guid}/complete")]
    [ProducesResponseType(typeof(ReferralDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReferralDto>> CompleteAsync(
        Guid referralId,
        [FromBody] CompleteReferralRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(ReferralCommandMapper.ToCompleteCommand(referralId, request), ct));
}
