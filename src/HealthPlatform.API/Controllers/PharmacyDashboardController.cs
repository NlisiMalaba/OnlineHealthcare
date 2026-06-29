using HealthPlatform.Application.PharmacyOrders.Dashboard;
using HealthPlatform.Application.PharmacyOrders.Dashboard.GetPharmacyDailySummary;
using HealthPlatform.Application.PharmacyOrders.Dashboard.GetPharmacyDashboard;
using HealthPlatform.Application.Security;using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/pharmacy/dashboard")]
[Authorize(Policy = AuthorizationPolicies.Pharmacy)]
public sealed class PharmacyDashboardController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PharmacyDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PharmacyDashboardDto>> GetDashboardAsync(CancellationToken ct) =>
        Ok(await sender.Send(new GetPharmacyDashboardQuery(), ct));

    [HttpGet("daily-summary")]
    [ProducesResponseType(typeof(PharmacyDailySummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PharmacyDailySummaryDto>> GetDailySummaryAsync(
        [FromQuery] DateOnly? date,
        CancellationToken ct) =>
        Ok(await sender.Send(new GetPharmacyDailySummaryQuery(date), ct));
}
