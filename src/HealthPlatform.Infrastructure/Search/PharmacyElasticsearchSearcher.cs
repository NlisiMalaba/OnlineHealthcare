using System.Text.Json;
using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using HealthPlatform.Application.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Search;

public sealed class PharmacyElasticsearchSearcher(
    ElasticsearchClient client,
    IOptions<ElasticsearchOptions> options,
    ILogger<PharmacyElasticsearchSearcher> logger)
{
    public async Task<PharmacySearchPageDto> SearchAsync(PharmacySearchCriteria criteria, CancellationToken ct)
    {
        var from = (criteria.Page - 1) * criteria.PageSize;
        var hasGeo = criteria.PatientLatitude.HasValue && criteria.PatientLongitude.HasValue;
        var requestBody = PharmacyElasticsearchSearchSupport.BuildSearchRequestBody(criteria, from, hasGeo);

        var response = await client.Transport.RequestAsync<StringResponse>(
            Elastic.Transport.HttpMethod.POST,
            $"{options.Value.PharmaciesIndex}/_search",
            PostData.String(requestBody),
            cancellationToken: ct);

        if (!response.ApiCallDetails.HasSuccessfulStatusCode)
        {
            logger.LogError("Pharmacy search query failed: {Error}", response.Body);
            throw new InvalidOperationException("Pharmacy search query failed.");
        }

        return PharmacyElasticsearchSearchSupport.ParseSearchResponse(response.Body, hasGeo);
    }
}
