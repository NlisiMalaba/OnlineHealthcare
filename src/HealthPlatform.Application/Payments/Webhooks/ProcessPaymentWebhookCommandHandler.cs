using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Domain.Payments;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Payments.Webhooks;

public sealed class ProcessPaymentWebhookCommandHandler(
    IPaymentGatewayResolver gatewayResolver,
    IPaymentWebhookIdempotencyStore idempotencyStore,
    IPaymentCompletionService paymentCompletionService,
    IAppointmentRepository appointmentRepository,
    IMedicationOrderRepository medicationOrderRepository,
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

        await DispatchSideEffectsAsync(gateway.ProviderName, parsed, ct);

        return new ProcessPaymentWebhookResultDto(
            Accepted: true,
            Duplicate: false,
            Status: parsed.Status);
    }

    private async Task DispatchSideEffectsAsync(
        string providerName,
        PaymentWebhookParseResultDto parsed,
        CancellationToken ct)
    {
        if (parsed.Status == PaymentWebhookEventStatus.Completed)
        {
            await CompletePaymentAsync(providerName, parsed, ct);
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

    private async Task CompletePaymentAsync(
        string providerName,
        PaymentWebhookParseResultDto parsed,
        CancellationToken ct)
    {
        if (parsed.AmountMinorUnits is not { } amountMinorUnits
            || string.IsNullOrWhiteSpace(parsed.Currency))
        {
            logger.LogWarning(
                "Skipping payment completion for provider payment {ProviderPaymentId} because amount or currency is missing.",
                parsed.ProviderPaymentId);
            return;
        }

        var patientId = await ResolvePatientIdAsync(parsed, ct);
        if (patientId is null)
        {
            logger.LogWarning(
                "Skipping payment completion for provider payment {ProviderPaymentId} because patient could not be resolved.",
                parsed.ProviderPaymentId);
            return;
        }

        var gatewayType = PaymentGatewayMapper.FromProviderName(providerName);
        var paymentMethod = PaymentGatewayMapper.DefaultMethodForGateway(gatewayType);

        await paymentCompletionService.CompleteAsync(
            new CompletePaymentRequest(
                patientId.Value,
                amountMinorUnits,
                parsed.Currency,
                paymentMethod,
                gatewayType,
                parsed.ProviderPaymentId,
                parsed.AppointmentId,
                parsed.MedicationOrderId,
                null,
                timeProvider.GetUtcNow().UtcDateTime),
            ct);
    }

    private async Task<Guid?> ResolvePatientIdAsync(PaymentWebhookParseResultDto parsed, CancellationToken ct)
    {
        if (parsed.AppointmentId is { } appointmentId)
        {
            var appointment = await appointmentRepository.GetByIdAsync(appointmentId, ct);
            return appointment?.PatientId;
        }

        if (parsed.MedicationOrderId is { } medicationOrderId)
        {
            var order = await medicationOrderRepository.GetByIdAsync(medicationOrderId, ct);
            return order?.PatientId;
        }

        return null;
    }
}
