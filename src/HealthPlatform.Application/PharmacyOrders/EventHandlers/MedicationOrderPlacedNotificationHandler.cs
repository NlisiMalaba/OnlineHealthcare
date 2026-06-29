using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.PharmacyOrders.Notifications;
using HealthPlatform.Application.PharmacyOrders.Realtime;
using HealthPlatform.Domain.Pharmacy;
using MediatR;

namespace HealthPlatform.Application.PharmacyOrders.EventHandlers;

public sealed class MedicationOrderPlacedNotificationHandler(
    IPharmacyRepository pharmacyRepository,
    IPharmacyOrderRealtimeNotifier realtimeNotifier,
    IPharmacyOrderReceivedNotifier receivedNotifier)
    : INotificationHandler<MedicationOrderPlacedNotification>
{
    public async Task Handle(MedicationOrderPlacedNotification notification, CancellationToken ct)
    {
        var pharmacy = await pharmacyRepository.GetByIdAsync(notification.PharmacyId, ct)
            ?? throw new NotFoundException(
                PharmacyErrorCodes.PharmacyNotFound,
                "Pharmacy profile was not found.");

        var realtimePayload = new PharmacyOrderReceivedRealtimeDto(
            notification.OrderId,
            notification.PatientId,
            notification.PrescriptionId,
            notification.MedicationSku,
            notification.MedicationName,
            notification.Dosage,
            notification.Frequency,
            notification.DurationDays,
            notification.SpecialInstructions,
            notification.DeliveryType.ToString().ToLowerInvariant(),
            notification.DeliveryAddress,
            notification.OccurredAtUtc);

        await realtimeNotifier.PublishOrderReceivedAsync(notification.PharmacyId, realtimePayload, ct);

        await receivedNotifier.NotifyOrderReceivedAsync(
            pharmacy.UserId,
            notification.OrderId,
            notification.PrescriptionId,
            notification.MedicationName,
            notification.DeliveryAddress,
            ct);
    }
}
