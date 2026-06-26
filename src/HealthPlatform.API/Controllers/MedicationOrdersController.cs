using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Pharmacy;
using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/pharmacy/orders")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class MedicationOrdersController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(MedicationOrderDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<MedicationOrderDto>> CreateAsync(
        [FromBody] CreateMedicationOrderRequest request,
        CancellationToken ct)
    {
        var order = await sender.Send(CreateMedicationOrderCommandMapper.ToCommand(request), ct);
        return Created($"/api/v1/pharmacy/orders/{order.Id}", order);
    }
}
