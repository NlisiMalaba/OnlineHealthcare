using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Pharmacy.Events;

public sealed record MedicationOrderPlacedDomainEvent(
    Guid OrderId,
    Guid PatientId,
    Guid PharmacyId,
    Guid PrescriptionId,
    string MedicationSku,
    string MedicationName,
    string Dosage,
    string Frequency,
    int DurationDays,
    string? SpecialInstructions,
    MedicationDeliveryType DeliveryType,
    string? DeliveryAddress) : DomainEvent;
