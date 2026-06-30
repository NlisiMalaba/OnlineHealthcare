using HealthPlatform.Domain.Payments;

namespace HealthPlatform.Application.Payments;

public sealed record RecordPaymentFailureRequest(
    Guid PatientId,
    long AmountMinorUnits,
    string Currency,
    PaymentMethod PaymentMethod,
    PaymentGatewayType Gateway,
    string? GatewayReference,
    Guid? AppointmentId,
    Guid? MedicationOrderId,
    Guid? LabOrderId,
    string FailureCode,
    string FailureMessage,
    DateTime FailedAtUtc);

public sealed record PaymentFailureResultDto(
    Guid PaymentId,
    DateTime RetentionExpiresAtUtc);

public interface IPaymentFailureService
{
    Task<PaymentFailureResultDto> RecordFailureAsync(RecordPaymentFailureRequest request, CancellationToken ct);
}
