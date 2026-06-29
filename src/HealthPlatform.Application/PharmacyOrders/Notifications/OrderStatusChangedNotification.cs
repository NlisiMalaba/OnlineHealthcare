using HealthPlatform.Domain.Pharmacy;
using HealthPlatform.Domain.Pharmacy.Events;
using MediatR;

namespace HealthPlatform.Application.PharmacyOrders.Notifications;

public sealed record OrderStatusChangedNotification(
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
    string? ClarificationMessage,
    DateTime OccurredAtUtc) : INotification;
