using HealthPlatform.Application.Payments.CreditLine;
using HealthPlatform.Application.Payments.CreditLine.GetCreditLine;
using HealthPlatform.Application.Payments.CreditLine.PayOnCreditLine;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/payments/credit-line")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class CreditLineController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(CreditLineDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreditLineDto>> GetAsync(CancellationToken ct) =>
        Ok(await sender.Send(new GetCreditLineQuery(), ct));

    [HttpPost("pay")]
    [ProducesResponseType(typeof(CreditLinePaymentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreditLinePaymentDto>> PayAsync(
        [FromBody] PayOnCreditLineRequest request,
        CancellationToken ct)
    {
        var payment = await sender.Send(
            new PayOnCreditLineCommand(
                request.AmountMinorUnits,
                request.Currency,
                request.AppointmentId,
                request.MedicationOrderId,
                request.LabOrderId),
            ct);

        return Created($"/api/v1/payments/credit-line/transactions/{payment.TransactionId}", payment);
    }
}

public sealed record PayOnCreditLineRequest(
    long AmountMinorUnits,
    string Currency,
    Guid? AppointmentId,
    Guid? MedicationOrderId,
    Guid? LabOrderId);
