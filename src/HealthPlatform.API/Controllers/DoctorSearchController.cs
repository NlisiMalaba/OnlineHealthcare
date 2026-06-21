using HealthPlatform.Application.Search.SearchDoctors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/search/doctors")]
[AllowAnonymous]
public sealed class DoctorSearchController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(SearchDoctorsResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchDoctorsResponseDto>> SearchAsync(
        [FromQuery] SearchDoctorsQuery query,
        CancellationToken ct) =>
        Ok(await sender.Send(query, ct));
}
