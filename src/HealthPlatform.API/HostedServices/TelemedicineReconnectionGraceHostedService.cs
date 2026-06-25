using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Telemedicine.Realtime.Reconnection;
using MediatR;

namespace HealthPlatform.API.HostedServices;

public sealed class TelemedicineReconnectionGraceHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<TelemedicineReconnectionGraceHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TelemedicinePolicies.ReconnectionGraceCheckInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                await sender.Send(new ProcessExpiredTelemedicineReconnectionGracesCommand(), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process expired telemedicine reconnection grace periods.");
            }
        }
    }
}
