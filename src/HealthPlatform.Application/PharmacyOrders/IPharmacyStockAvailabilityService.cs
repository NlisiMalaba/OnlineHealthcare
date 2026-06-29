namespace HealthPlatform.Application.PharmacyOrders;

public interface IPharmacyStockAvailabilityService
{
    Task<bool> HasMedicationInStockAsync(
        Guid pharmacyId,
        string medicationSku,
        CancellationToken ct);
}
