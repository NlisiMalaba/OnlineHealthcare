using HealthPlatform.Application.Search.SearchLabPartners;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/search/lab-partners")]
[AllowAnonymous]
public sealed class LabPartnerSearchController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(SearchLabPartnersResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchLabPartnersResponseDto>> SearchAsync(
        [FromQuery] SearchLabPartnersQuery query,
        CancellationToken ct) =>
        Ok(await sender.Send(query, ct));
}
