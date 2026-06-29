using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.PharmacyOrders.Notifications;
using MediatR;

namespace HealthPlatform.Application.PharmacyOrders.EventHandlers;

public sealed class InventoryLowStockDetectedNotificationHandler(
    IPharmacyRepository pharmacyRepository,
    ILowStockAlertNotifier lowStockAlertNotifier)
    : INotificationHandler<InventoryLowStockDetectedNotification>
{
    public async Task Handle(InventoryLowStockDetectedNotification notification, CancellationToken ct)
    {
        var pharmacy = await pharmacyRepository.GetByIdAsync(notification.PharmacyId, ct)
            ?? throw new NotFoundException(
                PharmacyErrorCodes.PharmacyNotFound,
                "Pharmacy profile was not found.");

        await lowStockAlertNotifier.NotifyLowStockAsync(
            pharmacy.UserId,
            notification.InventoryItemId,
            notification.MedicationSku,
            notification.MedicationName,
            notification.Quantity,
            notification.LowStockThreshold,
            ct);
    }
}
