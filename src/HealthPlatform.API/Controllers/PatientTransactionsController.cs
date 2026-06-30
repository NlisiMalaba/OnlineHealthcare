using HealthPlatform.Application.Payments;
using HealthPlatform.Application.Payments.GetPaymentReceipt;
using HealthPlatform.Application.Payments.ListPatientTransactionHistory;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/payments")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class PatientTransactionsController(ISender sender) : ControllerBase
{
    [HttpGet("transactions")]
    [ProducesResponseType(typeof(IReadOnlyList<PatientTransactionHistoryItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PatientTransactionHistoryItemDto>>> ListTransactionsAsync(
        CancellationToken ct) =>
        Ok(await sender.Send(new ListPatientTransactionHistoryQuery(), ct));

    [HttpGet("{paymentId:guid}/receipt")]
    [ProducesResponseType(typeof(PaymentReceiptDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaymentReceiptDto>> GetReceiptAsync(
        Guid paymentId,
        CancellationToken ct) =>
        Ok(await sender.Send(new GetPaymentReceiptQuery(paymentId), ct));
}
