namespace HealthPlatform.Infrastructure.Search.Documents;

/// <summary>
/// Elasticsearch read model for lab partner discovery (Requirement 21.3).
/// </summary>
public sealed class LabPartnerSearchDocument
{
    public string LabPartnerId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public GeoLocationDocument? Location { get; init; }

    public IReadOnlyList<string> TestTypes { get; init; } = [];

    public IReadOnlyList<LabTestPricingEntry> Pricing { get; init; } = [];

    public bool IsSearchable { get; init; }
}
