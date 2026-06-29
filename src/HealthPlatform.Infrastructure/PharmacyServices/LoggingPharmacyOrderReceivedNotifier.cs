using HealthPlatform.Application.PharmacyOrders;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.PharmacyServices;

public sealed class LoggingPharmacyOrderReceivedNotifier(
    ILogger<LoggingPharmacyOrderReceivedNotifier> logger)
    : IPharmacyOrderReceivedNotifier
{
    public Task NotifyOrderReceivedAsync(
        Guid pharmacyUserId,
        Guid orderId,
        Guid prescriptionId,
        string medicationName,
        string? deliveryAddress,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        logger.LogInformation(
            "Medication order {OrderId} notification queued for pharmacy user {PharmacyUserId}. Prescription {PrescriptionId}, medication {MedicationName}, delivery address present: {HasDeliveryAddress}.",
            orderId,
            pharmacyUserId,
            prescriptionId,
            medicationName,
            !string.IsNullOrWhiteSpace(deliveryAddress));

        return Task.CompletedTask;
    }
}
