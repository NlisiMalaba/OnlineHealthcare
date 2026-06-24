using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using HealthPlatform.Infrastructure.Search.Indices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Search;

public sealed class ElasticsearchIndexManager(
    ElasticsearchClient client,
    IOptions<ElasticsearchOptions> options,
    ILogger<ElasticsearchIndexManager> logger) : IElasticsearchIndexManager
{
    public async Task EnsureIndicesCreatedAsync(CancellationToken ct)
    {
        await EnsureIndexAsync(options.Value.DoctorsIndex, SearchIndexMappings.Doctor, ct);
        await EnsureIndexAsync(options.Value.PharmaciesIndex, SearchIndexMappings.Pharmacy, ct);
        await EnsureIndexAsync(options.Value.LabPartnersIndex, SearchIndexMappings.LabPartner, ct);
    }

    private async Task EnsureIndexAsync(string indexName, string mappingJson, CancellationToken ct)
    {
        var existsResponse = await client.Indices.ExistsAsync(indexName, ct);
        if (existsResponse.Exists)
        {
            logger.LogDebug("Elasticsearch index {IndexName} already exists.", indexName);
            return;
        }

        var requestBody = JsonSerializer.Serialize(new
        {
            settings = new
            {
                number_of_shards = 1,
                number_of_replicas = 0
            },
            mappings = JsonSerializer.Deserialize<JsonElement>(mappingJson)
        });

        var createResponse = await client.Transport.RequestAsync<StringResponse>(
            Elastic.Transport.HttpMethod.PUT,
            indexName,
            PostData.String(requestBody),
            cancellationToken: ct);

        if (!createResponse.ApiCallDetails.HasSuccessfulStatusCode)
        {
            logger.LogError(
                "Failed to create Elasticsearch index {IndexName}: {Error}",
                indexName,
                createResponse.Body);
            throw new InvalidOperationException($"Failed to create Elasticsearch index '{indexName}'.");
        }

        logger.LogInformation("Created Elasticsearch index {IndexName}.", indexName);
    }
}
