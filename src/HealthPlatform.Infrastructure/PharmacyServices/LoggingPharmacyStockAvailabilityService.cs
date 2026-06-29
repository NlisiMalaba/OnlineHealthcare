using HealthPlatform.Application.PharmacyOrders;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.PharmacyServices;

public sealed class LoggingPharmacyStockAvailabilityService(
    ILogger<LoggingPharmacyStockAvailabilityService> logger)
    : IPharmacyStockAvailabilityService
{
    public Task<bool> HasMedicationInStockAsync(
        Guid pharmacyId,
        string medicationSku,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        logger.LogInformation(
            "Stock availability check for pharmacy {PharmacyId} and sku {MedicationSku} returned unavailable in logging mode.",
            pharmacyId,
            medicationSku);

        return Task.FromResult(false);
    }
}
