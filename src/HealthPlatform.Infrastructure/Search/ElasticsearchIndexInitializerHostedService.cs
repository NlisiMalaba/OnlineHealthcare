using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Search;

public sealed class ElasticsearchIndexInitializerHostedService(
    IServiceProvider serviceProvider,
    IOptions<ElasticsearchOptions> options,
    ILogger<ElasticsearchIndexInitializerHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.EnsureIndicesOnStartup)
        {
            logger.LogDebug("Elasticsearch index initialization is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(options.Value.Uri))
        {
            logger.LogDebug("Elasticsearch URI is not configured; skipping index initialization.");
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var indexManager = scope.ServiceProvider.GetRequiredService<IElasticsearchIndexManager>();

        try
        {
            await indexManager.EnsureIndicesCreatedAsync(cancellationToken);
            logger.LogInformation("Elasticsearch search indices are ready.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Elasticsearch index initialization failed.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
