using HealthPlatform.Domain.Pharmacy;
using HealthPlatform.Domain.Pharmacy.Events;
using MediatR;

namespace HealthPlatform.Application.PharmacyOrders.Notifications;

public sealed record MedicationOrderPlacedNotification(
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
    string? DeliveryAddress,
    DateTime OccurredAtUtc) : INotification;
