using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Application.PharmacyOrders.Realtime;

namespace HealthPlatform.Tests.Support;

public sealed class FakePharmacyStockAvailabilityService : IPharmacyStockAvailabilityService
{
    private readonly HashSet<(Guid PharmacyId, string MedicationSku)> _inStock = [];

    public void SetInStock(Guid pharmacyId, string medicationSku) =>
        _inStock.Add((pharmacyId, medicationSku.Trim()));

    public Task<bool> HasMedicationInStockAsync(
        Guid pharmacyId,
        string medicationSku,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_inStock.Contains((pharmacyId, medicationSku.Trim())));
    }
}
