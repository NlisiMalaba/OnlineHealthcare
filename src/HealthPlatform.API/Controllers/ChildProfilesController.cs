using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Maternal;
using HealthPlatform.Application.Maternal.ChildProfiles;
using HealthPlatform.Application.Maternal.ChildProfiles.CreateChildProfile;
using HealthPlatform.Application.Maternal.ChildProfiles.GetChildProfile;
using HealthPlatform.Application.Maternal.ChildProfiles.ListChildProfiles;
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
}
