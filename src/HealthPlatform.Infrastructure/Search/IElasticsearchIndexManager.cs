namespace HealthPlatform.Infrastructure.Search;

public interface IElasticsearchIndexManager
{
    Task EnsureIndicesCreatedAsync(CancellationToken ct);
}
