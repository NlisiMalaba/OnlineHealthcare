using Elastic.Clients.Elasticsearch;
using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Infrastructure.Search;
using HealthPlatform.Infrastructure.Search.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.PharmacyServices;

public sealed class PharmacyStockAvailabilityService(
    ElasticsearchClient client,
    IOptions<ElasticsearchOptions> options,
    ILogger<PharmacyStockAvailabilityService> logger) : IPharmacyStockAvailabilityService
{
    public async Task<bool> HasMedicationInStockAsync(
        Guid pharmacyId,
        string medicationSku,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(medicationSku))
        {
            return false;
        }

        var normalizedSku = medicationSku.Trim();
        var response = await client.GetAsync<PharmacySearchDocument>(
            options.Value.PharmaciesIndex,
            pharmacyId.ToString(),
            ct);

        if (!response.Found || response.Source?.StockSummary is null)
        {
            logger.LogWarning(
                "Pharmacy {PharmacyId} missing from search index during stock availability check.",
                pharmacyId);
            return false;
        }

        return response.Source.StockSummary.Any(entry =>
            string.Equals(entry.MedicationSku, normalizedSku, StringComparison.Ordinal)
            && entry.QuantityOnHand > 0);
    }
}
