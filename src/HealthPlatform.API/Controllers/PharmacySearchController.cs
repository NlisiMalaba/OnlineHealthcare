using HealthPlatform.Application.Search.SearchPharmacies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/search/pharmacies")]
[AllowAnonymous]
public sealed class PharmacySearchController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(SearchPharmaciesResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchPharmaciesResponseDto>> SearchAsync(
        [FromQuery] SearchPharmaciesQuery query,
        CancellationToken ct) =>
        Ok(await sender.Send(query, ct));
}
