using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Application.PharmacyOrders.Realtime;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingPharmacyOrderRealtimeNotifier : IPharmacyOrderRealtimeNotifier
{
    public List<(Guid PharmacyId, PharmacyOrderReceivedRealtimeDto Order)> Published { get; } = [];

    public Task PublishOrderReceivedAsync(
        Guid pharmacyId,
        PharmacyOrderReceivedRealtimeDto order,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Published.Add((pharmacyId, order));
        return Task.CompletedTask;
    }
}

public sealed class CapturingPharmacyOrderReceivedNotifier : IPharmacyOrderReceivedNotifier
{
    public List<(Guid PharmacyUserId, Guid OrderId, Guid PrescriptionId, string MedicationName, string? DeliveryAddress)> Notifications { get; } = [];

    public Task NotifyOrderReceivedAsync(
        Guid pharmacyUserId,
        Guid orderId,
        Guid prescriptionId,
        string medicationName,
        string? deliveryAddress,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Notifications.Add((pharmacyUserId, orderId, prescriptionId, medicationName, deliveryAddress));
        return Task.CompletedTask;
    }
}
