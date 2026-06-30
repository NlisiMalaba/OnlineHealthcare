using HealthPlatform.Domain.Payments;

namespace HealthPlatform.Application.Payments;

public sealed record CompletePaymentRequest(
    Guid PatientId,
    long AmountMinorUnits,
    string Currency,
    PaymentMethod PaymentMethod,
    PaymentGatewayType Gateway,
    string? GatewayReference,
    Guid? AppointmentId,
    Guid? MedicationOrderId,
    Guid? LabOrderId,
    DateTime CompletedAtUtc);

public sealed record PaymentCompletionResultDto(
    Guid PaymentId,
    string ReceiptStorageKey,
    string ReceiptReadUrl);

public sealed record PaymentReceiptDto(
    Guid PaymentId,
    string ReceiptReadUrl);

public enum PatientTransactionCategory
{
    ConsultationFee = 0,
    MedicationCost = 1,
    LabTestCharge = 2,
    InsuranceClaim = 3,
    CreditLine = 4,
    InstalmentPlan = 5
}

public sealed record PatientTransactionHistoryItemDto(
    Guid EntryId,
    PatientTransactionCategory Category,
    string Description,
    long AmountMinorUnits,
    string Currency,
    string Status,
    DateTime OccurredAtUtc,
    Guid? PaymentId,
    string? ReceiptUrl);
