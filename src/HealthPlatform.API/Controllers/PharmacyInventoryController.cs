using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Pharmacy;
using HealthPlatform.Application.PharmacyOrders.Inventory;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/pharmacy/inventory")]
[Authorize(Policy = AuthorizationPolicies.Pharmacy)]
public sealed class PharmacyInventoryController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(InventoryItemDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<InventoryItemDto>> AddAsync(
        [FromBody] AddInventoryItemRequest request,
        CancellationToken ct)
    {
        var item = await sender.Send(PharmacyInventoryCommandMapper.ToAddCommand(request), ct);
        return Created($"/api/v1/pharmacy/inventory/{item.Id}", item);
    }

    [HttpPut("{inventoryItemId:guid}/quantity")]
    [ProducesResponseType(typeof(InventoryItemDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventoryItemDto>> UpdateQuantityAsync(
        Guid inventoryItemId,
        [FromBody] UpdateInventoryItemQuantityRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(
            PharmacyInventoryCommandMapper.ToUpdateQuantityCommand(inventoryItemId, request),
            ct));

    [HttpPost("{inventoryItemId:guid}/mark-out-of-stock")]
    [ProducesResponseType(typeof(InventoryItemDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventoryItemDto>> MarkOutOfStockAsync(
        Guid inventoryItemId,
        CancellationToken ct) =>
        Ok(await sender.Send(
            PharmacyInventoryCommandMapper.ToMarkOutOfStockCommand(inventoryItemId),
            ct));
}
