namespace HealthPlatform.Infrastructure.Search.Documents;

/// <summary>
/// Elasticsearch read model for pharmacy discovery (Requirement 7).
/// </summary>
public sealed class PharmacySearchDocument
{
    public string PharmacyId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public GeoLocationDocument? Location { get; init; }

    public IReadOnlyList<PharmacyStockSummaryEntry> StockSummary { get; init; } = [];

    public bool HasStock { get; init; }

    public bool IsSearchable { get; init; }
}
