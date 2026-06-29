namespace HealthPlatform.Application.PharmacyOrders;

public interface IPharmacyOrderReceivedNotifier
{
    Task NotifyOrderReceivedAsync(
        Guid pharmacyUserId,
        Guid orderId,
        Guid prescriptionId,
        string medicationName,
        string? deliveryAddress,
        CancellationToken ct);
}
