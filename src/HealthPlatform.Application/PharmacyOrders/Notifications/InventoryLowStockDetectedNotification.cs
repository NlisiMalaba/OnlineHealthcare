using MediatR;

namespace HealthPlatform.Application.PharmacyOrders.Notifications;

public sealed record InventoryLowStockDetectedNotification(
    Guid InventoryItemId,
    Guid PharmacyId,
    string MedicationSku,
    string MedicationName,
    int Quantity,
    int LowStockThreshold,
    DateTime OccurredAtUtc) : INotification;
