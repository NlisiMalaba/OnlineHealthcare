using HealthPlatform.Application.Payments;
using HealthPlatform.Application.Payments.Webhooks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/payments/webhooks")]
[AllowAnonymous]
public sealed class PaymentWebhooksController(ISender sender) : ControllerBase
{
    [HttpPost("stripe")]
    [ProducesResponseType(typeof(ProcessPaymentWebhookResultDto), StatusCodes.Status200OK)]
    public Task<ActionResult<ProcessPaymentWebhookResultDto>> StripeAsync(CancellationToken ct) =>
        ProcessAsync(PaymentGatewayProviders.Stripe, ct);

    [HttpPost("flutterwave")]
    [ProducesResponseType(typeof(ProcessPaymentWebhookResultDto), StatusCodes.Status200OK)]
    public Task<ActionResult<ProcessPaymentWebhookResultDto>> FlutterwaveAsync(CancellationToken ct) =>
        ProcessAsync(PaymentGatewayProviders.Flutterwave, ct);

    [HttpPost("paystack")]
    [ProducesResponseType(typeof(ProcessPaymentWebhookResultDto), StatusCodes.Status200OK)]
    public Task<ActionResult<ProcessPaymentWebhookResultDto>> PaystackAsync(CancellationToken ct) =>
        ProcessAsync(PaymentGatewayProviders.Paystack, ct);

    [HttpPost("mpesa")]
    [ProducesResponseType(typeof(ProcessPaymentWebhookResultDto), StatusCodes.Status200OK)]
    public Task<ActionResult<ProcessPaymentWebhookResultDto>> MpesaAsync(CancellationToken ct) =>
        ProcessAsync(PaymentGatewayProviders.Mpesa, ct);

    private async Task<ActionResult<ProcessPaymentWebhookResultDto>> ProcessAsync(
        string provider,
        CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(ct);
        var headers = Request.Headers.ToDictionary(
            header => header.Key,
            header => header.Value.ToString(),
            StringComparer.OrdinalIgnoreCase);

        var result = await sender.Send(
            new ProcessPaymentWebhookCommand(provider, rawBody, headers),
            ct);

        return Ok(result);
    }
}
