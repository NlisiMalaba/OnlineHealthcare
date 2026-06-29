namespace HealthPlatform.Application.Payments;

public enum PaymentWebhookEventStatus
{
    Ignored = 0,
    Completed = 1,
    Failed = 2
}

public sealed record PaymentWebhookParseRequestDto(
    string RawBody,
    IReadOnlyDictionary<string, string> Headers);

public sealed record PaymentWebhookParseResultDto(
    bool SignatureValid,
    string? EventId,
    string? ProviderPaymentId,
    PaymentWebhookEventStatus Status,
    long? AmountMinorUnits,
    string? Currency,
    Guid? AppointmentId,
    Guid? MedicationOrderId,
    string? FailureCode,
    string? FailureMessage);
