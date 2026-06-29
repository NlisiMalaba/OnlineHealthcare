using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Payments.Webhooks;

public sealed class ProcessPaymentWebhookCommandHandler(
    IPaymentGatewayResolver gatewayResolver,
    IPaymentWebhookIdempotencyStore idempotencyStore,
    IMediator mediator,
    TimeProvider timeProvider,
    ILogger<ProcessPaymentWebhookCommandHandler> logger)
    : IRequestHandler<ProcessPaymentWebhookCommand, ProcessPaymentWebhookResultDto>
{
    public async Task<ProcessPaymentWebhookResultDto> Handle(
        ProcessPaymentWebhookCommand request,
        CancellationToken ct)
    {
        var gateway = gatewayResolver.GetRequired(request.ProviderName);
        var parsed = await gateway.ParseWebhookAsync(
            new PaymentWebhookParseRequestDto(request.RawBody, request.Headers),
            ct);

        if (!parsed.SignatureValid)
        {
            throw new AccessDeniedException(
                "WEBHOOK_SIGNATURE_INVALID",
                "Payment webhook signature validation failed.");
        }

        if (parsed.Status == PaymentWebhookEventStatus.Ignored
            || string.IsNullOrWhiteSpace(parsed.EventId))
        {
            return new ProcessPaymentWebhookResultDto(Accepted: true, Duplicate: false, Status: parsed.Status);
        }

        if (!await idempotencyStore.TryBeginProcessingAsync(
                gateway.ProviderName,
                parsed.EventId,
                ct))
        {
            logger.LogInformation(
                "Ignoring duplicate payment webhook {Provider} event {EventId}.",
                gateway.ProviderName,
                parsed.EventId);

            return new ProcessPaymentWebhookResultDto(
                Accepted: true,
                Duplicate: true,
                Status: parsed.Status);
        }

        await DispatchSideEffectsAsync(parsed, ct);

        return new ProcessPaymentWebhookResultDto(
            Accepted: true,
            Duplicate: false,
            Status: parsed.Status);
    }

    private async Task DispatchSideEffectsAsync(PaymentWebhookParseResultDto parsed, CancellationToken ct)
    {
        if (parsed.Status == PaymentWebhookEventStatus.Completed && parsed.AppointmentId is { } appointmentId)
        {
            var paymentId = Guid.CreateVersion7();
            await mediator.Publish(
                new PaymentCompletedNotification(
                    appointmentId,
                    paymentId,
                    timeProvider.GetUtcNow().UtcDateTime),
                ct);
        }

        if (parsed.Status == PaymentWebhookEventStatus.Failed)
        {
            logger.LogWarning(
                "Payment webhook reported failure for provider payment {ProviderPaymentId}: {FailureCode} {FailureMessage}",
                parsed.ProviderPaymentId,
                parsed.FailureCode,
                parsed.FailureMessage);
        }
    }
}
