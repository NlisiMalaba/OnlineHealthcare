namespace HealthPlatform.Infrastructure.Search.Documents;

public sealed class PharmacyStockSummaryEntry
{
    public string MedicationName { get; init; } = string.Empty;

    public string MedicationSku { get; init; } = string.Empty;

    public int QuantityOnHand { get; init; }
}
