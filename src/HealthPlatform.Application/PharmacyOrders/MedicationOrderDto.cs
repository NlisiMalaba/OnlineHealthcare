namespace HealthPlatform.Application.PharmacyOrders;

public sealed record MedicationOrderDto(
    Guid Id,
    Guid PatientId,
    Guid PharmacyId,
    Guid PrescriptionId,
    string MedicationSku,
    string MedicationName,
    string Dosage,
    string Frequency,
    int DurationDays,
    string? SpecialInstructions,
    string DeliveryType,
    string? DeliveryAddress,
    string Status,
    string? DeliveryAgentName,
    string? TrackingUrl,
    string? RejectionReason,
    string? ClarificationMessage,
    DateTime CreatedAtUtc);
