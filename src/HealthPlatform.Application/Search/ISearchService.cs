namespace HealthPlatform.Application.Search;

public sealed record PharmacyStockIndexEntry(
    string MedicationName,
    string MedicationSku,
    int QuantityOnHand);

public interface ISearchService
{
    Task UpsertDoctorAsync(Guid doctorId, CancellationToken ct);

    Task RemoveDoctorAsync(Guid doctorId, CancellationToken ct);

    Task UpsertPharmacyAsync(Guid pharmacyId, CancellationToken ct);

    Task UpdatePharmacyStockAsync(
        Guid pharmacyId,
        IReadOnlyList<PharmacyStockIndexEntry> stockSummary,
        CancellationToken ct);
}
