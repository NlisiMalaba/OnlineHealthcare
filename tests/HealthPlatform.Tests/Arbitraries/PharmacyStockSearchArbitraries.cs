using FsCheck;
using HealthPlatform.Domain.ValueObjects;
using HealthPlatform.Tests.Generators;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record PharmacyStockLine(string MedicationSku, int QuantityOnHand);

public sealed record IndexedPharmacyForSearch(
    Guid PharmacyId,
    string Name,
    string Address,
    GeoPoint? Location,
    bool IsSearchable,
    IReadOnlyList<PharmacyStockLine> StockSummary);

public sealed record PharmacyStockSearchCase(
    string MedicationSku,
    GeoPoint? PatientLocation,
    IReadOnlyList<IndexedPharmacyForSearch> Pharmacies);

public static class PharmacyStockSearchArbitraries
{
    private static readonly string[] MedicationSkus =
    [
        "MED-001",
        "MED-002",
        "MED-003",
        "MED-004",
        "MED-005",
        "MED-006",
        "MED-007",
        "MED-008"
    ];

    public static Arbitrary<PharmacyStockSearchCase> StockSearchCase() =>
        (from medicationSku in Gen.Elements(MedicationSkus)
         from includeGeo in Arb.Generate<bool>()
         from patientLocation in (includeGeo
             ? DomainGenerators.WildGeoPoint().Generator.Select(point => (GeoPoint?)point)
             : Gen.Constant<GeoPoint?>(null))
         from pharmacyCount in Gen.Choose(1, 15)
         from pharmacies in Gen.ListOf(pharmacyCount, IndexedPharmacy().Generator)
         select new PharmacyStockSearchCase(
             medicationSku,
             patientLocation,
             pharmacies.ToList()))
        .ToArbitrary();

    private static Arbitrary<IndexedPharmacyForSearch> IndexedPharmacy() =>
        (from pharmacyId in DomainGenerators.NonEmptyGuid().Generator
         from name in Gen.Elements("City Pharmacy", "Central Chemist", "HealthPlus", "MediCare")
         from address in Gen.Elements("12 Main St", "45 Jason Moyo Ave", "7 First Street")
         from hasLocation in Arb.Generate<bool>()
         from location in (hasLocation
             ? DomainGenerators.WildGeoPoint().Generator.Select(point => (GeoPoint?)point)
             : Gen.Constant<GeoPoint?>(null))
         from isSearchable in Gen.OneOf(Gen.Constant(true), Gen.Constant(false))
         from stockLineCount in Gen.Choose(0, 6)
         from stockLines in Gen.ListOf(stockLineCount, StockLine().Generator)
         select new IndexedPharmacyForSearch(
             pharmacyId,
             $"{name} {pharmacyId:N}"[..24],
             address,
             location,
             isSearchable,
             stockLines.ToList()))
        .ToArbitrary();

    private static Arbitrary<PharmacyStockLine> StockLine() =>
        (from medicationSku in Gen.Elements(MedicationSkus)
         from quantity in Gen.Choose(0, 250)
         select new PharmacyStockLine(medicationSku, quantity))
        .ToArbitrary();
}
