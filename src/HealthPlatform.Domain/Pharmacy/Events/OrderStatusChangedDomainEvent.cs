using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Pharmacy.Events;

public sealed record OrderStatusChangedDomainEvent(
    Guid OrderId,
    Guid PatientId,
    Guid PharmacyId,
    string MedicationSku,
    MedicationOrderStatus PreviousStatus,
    MedicationOrderStatus NewStatus,
    MedicationDeliveryType DeliveryType,
    string? TrackingUrl,
    string? DeliveryAgentName,
    string? RejectionReason,
    string? ClarificationMessage) : DomainEvent;
