namespace HealthPlatform.Infrastructure.Search.Documents;

public sealed class LabTestPricingEntry
{
    public string TestType { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public string Currency { get; init; } = "USD";
}
