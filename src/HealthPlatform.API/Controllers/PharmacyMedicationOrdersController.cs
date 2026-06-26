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
[Authorize(Policy = AuthorizationPolicies.Pharmacy)]
public sealed class PharmacyMedicationOrdersController(ISender sender) : ControllerBase
{
    [HttpPost("{orderId:guid}/confirm")]
    [ProducesResponseType(typeof(MedicationOrderDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MedicationOrderDto>> ConfirmAsync(Guid orderId, CancellationToken ct) =>
        Ok(await sender.Send(PharmacyMedicationOrderCommandMapper.ToConfirmCommand(orderId), ct));

    [HttpPost("{orderId:guid}/reject")]
    [ProducesResponseType(typeof(MedicationOrderDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MedicationOrderDto>> RejectAsync(
        Guid orderId,
        [FromBody] RejectMedicationOrderRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(PharmacyMedicationOrderCommandMapper.ToRejectCommand(orderId, request), ct));

    [HttpPost("{orderId:guid}/request-clarification")]
    [ProducesResponseType(typeof(MedicationOrderDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MedicationOrderDto>> RequestClarificationAsync(
        Guid orderId,
        [FromBody] RequestMedicationOrderClarificationRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(
            PharmacyMedicationOrderCommandMapper.ToClarificationCommand(orderId, request),
            ct));

    [HttpPost("{orderId:guid}/mark-dispatched")]
    [ProducesResponseType(typeof(MedicationOrderDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MedicationOrderDto>> MarkDispatchedAsync(Guid orderId, CancellationToken ct) =>
        Ok(await sender.Send(PharmacyMedicationOrderCommandMapper.ToDispatchedCommand(orderId), ct));

    [HttpPost("{orderId:guid}/complete-fulfillment")]
    [ProducesResponseType(typeof(MedicationOrderDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MedicationOrderDto>> CompleteFulfillmentAsync(Guid orderId, CancellationToken ct) =>
        Ok(await sender.Send(PharmacyMedicationOrderCommandMapper.ToFulfillmentCommand(orderId), ct));
}
