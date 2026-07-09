using HealthPlatform.Application.Queue;

namespace HealthPlatform.API.HostedServices;

public sealed class QueueRealtimeTickerHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<QueueRealtimeTickerHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(QueuePolicies.RealtimeUpdateInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IQueueRealtimeDispatcher>();
                await dispatcher.DispatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish queue realtime updates.");
            }
        }
    }
}
