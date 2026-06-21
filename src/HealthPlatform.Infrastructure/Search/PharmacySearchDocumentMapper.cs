using HealthPlatform.Application.Search;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.Search.Documents;

namespace HealthPlatform.Infrastructure.Search;

internal static class PharmacySearchDocumentMapper
{
    public static PharmacySearchDocument Map(
        Pharmacy pharmacy,
        IReadOnlyList<PharmacyStockIndexEntry>? stockSummary = null)
    {
        var stock = (stockSummary ?? [])
            .Select(entry => new PharmacyStockSummaryEntry
            {
                MedicationName = entry.MedicationName,
                MedicationSku = entry.MedicationSku,
                QuantityOnHand = entry.QuantityOnHand
            })
            .ToList();

        return new PharmacySearchDocument
        {
            PharmacyId = pharmacy.Id.ToString(),
            Name = pharmacy.Name,
            Address = pharmacy.Address,
            Location = GeoLocationDocument.FromGeoPoint(pharmacy.Location),
            StockSummary = stock,
            HasStock = stock.Any(entry => entry.QuantityOnHand > 0),
            IsSearchable = pharmacy.VerificationStatus == PharmacyVerificationStatus.Verified
        };
    }

    public static IReadOnlyList<PharmacyStockSummaryEntry> MapStock(
        IReadOnlyList<PharmacyStockIndexEntry> stockSummary) =>
        stockSummary
            .Select(entry => new PharmacyStockSummaryEntry
            {
                MedicationName = entry.MedicationName,
                MedicationSku = entry.MedicationSku,
                QuantityOnHand = entry.QuantityOnHand
            })
            .ToList();
}
