using System.Text.Json;
using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using HealthPlatform.Application.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Search;

public sealed class LabPartnerElasticsearchSearcher(
    ElasticsearchClient client,
    IOptions<ElasticsearchOptions> options,
    ILogger<LabPartnerElasticsearchSearcher> logger)
{
    public async Task<LabPartnerSearchPageDto> SearchAsync(LabPartnerSearchCriteria criteria, CancellationToken ct)
    {
        var from = (criteria.Page - 1) * criteria.PageSize;
        var hasGeo = criteria.PatientLatitude.HasValue && criteria.PatientLongitude.HasValue;
        var requestBody = LabPartnerElasticsearchSearchSupport.BuildSearchRequestBody(criteria, from, hasGeo);

        var response = await client.Transport.RequestAsync<StringResponse>(
            Elastic.Transport.HttpMethod.POST,
            $"{options.Value.LabPartnersIndex}/_search",
            PostData.String(requestBody),
            cancellationToken: ct);

        if (!response.ApiCallDetails.HasSuccessfulStatusCode)
        {
            logger.LogError("Lab partner search query failed: {Error}", response.Body);
            throw new InvalidOperationException("Lab partner search query failed.");
        }

        return LabPartnerElasticsearchSearchSupport.ParseSearchResponse(
            response.Body,
            hasGeo,
            criteria.TestType);
    }
}
