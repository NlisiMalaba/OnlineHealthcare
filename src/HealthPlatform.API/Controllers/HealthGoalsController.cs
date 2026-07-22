using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Wellness;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Wellness.HealthGoals;
using HealthPlatform.Application.Wellness.HealthGoals.CreateHealthGoal;
using HealthPlatform.Application.Wellness.HealthGoals.DeleteHealthGoal;
using HealthPlatform.Application.Wellness.HealthGoals.GetHealthGoal;
using HealthPlatform.Application.Wellness.HealthGoals.ListHealthGoals;
using HealthPlatform.Application.Wellness.HealthGoals.UpdateHealthGoal;
using HealthPlatform.Domain.Wellness;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/wellness/goals")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class HealthGoalsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HealthGoalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HealthGoalDto>>> ListAsync(
        [FromQuery] HealthGoalStatus? status,
        CancellationToken ct) =>
        Ok(await sender.Send(new ListHealthGoalsQuery(status), ct));

    [HttpGet("{goalId:guid}")]
    [ProducesResponseType(typeof(HealthGoalDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthGoalDto>> GetAsync(Guid goalId, CancellationToken ct) =>
        Ok(await sender.Send(new GetHealthGoalQuery(goalId), ct));

    [HttpPost]
    [ProducesResponseType(typeof(HealthGoalDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<HealthGoalDto>> CreateAsync(
        [FromBody] CreateHealthGoalRequest request,
        CancellationToken ct)
    {
        var goal = await sender.Send(WellnessCommandMapper.ToCreateGoalCommand(request), ct);
        return CreatedAtAction(nameof(GetAsync), new { goalId = goal.Id }, goal);
    }

    [HttpPut("{goalId:guid}")]
    [ProducesResponseType(typeof(HealthGoalDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthGoalDto>> UpdateAsync(
        Guid goalId,
        [FromBody] UpdateHealthGoalRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(WellnessCommandMapper.ToUpdateGoalCommand(goalId, request), ct));

    [HttpDelete("{goalId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(Guid goalId, CancellationToken ct)
    {
        await sender.Send(new DeleteHealthGoalCommand(goalId), ct);
        return NoContent();
    }
}
