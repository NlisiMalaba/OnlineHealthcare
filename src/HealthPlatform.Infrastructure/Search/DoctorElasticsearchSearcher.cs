using System.Text.Json;
using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using HealthPlatform.Application.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Search;

public sealed class DoctorElasticsearchSearcher(
    ElasticsearchClient client,
    IOptions<ElasticsearchOptions> options,
    ILogger<DoctorElasticsearchSearcher> logger)
{
    public async Task<DoctorSearchPageDto> SearchAsync(DoctorSearchCriteria criteria, CancellationToken ct)
    {
        var from = (criteria.Page - 1) * criteria.PageSize;
        var hasGeo = criteria.PatientLatitude.HasValue && criteria.PatientLongitude.HasValue;
        var requestBody = DoctorElasticsearchSearchSupport.BuildSearchRequestBody(criteria, from, hasGeo);

        var response = await client.Transport.RequestAsync<StringResponse>(
            Elastic.Transport.HttpMethod.POST,
            $"{options.Value.DoctorsIndex}/_search",
            PostData.String(requestBody),
            cancellationToken: ct);

        if (!response.ApiCallDetails.HasSuccessfulStatusCode)
        {
            logger.LogError(
                "Doctor search query failed: {Error}",
                response.Body);
            throw new InvalidOperationException("Doctor search query failed.");
        }

        return DoctorElasticsearchSearchSupport.ParseSearchResponse(response.Body, hasGeo);
    }
}
