using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Telemedicine.Realtime;
using HealthPlatform.Domain.Telemedicine;

namespace HealthPlatform.API.HostedServices;

public sealed class TelemedicineSessionDurationTickerHostedService(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<TelemedicineSessionDurationTickerHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TelemedicinePolicies.DurationTickInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await PublishTicksAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish telemedicine session duration ticks.");
            }
        }
    }

    private async Task PublishTicksAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var sessions = scope.ServiceProvider.GetRequiredService<ITelemedicineSessionRepository>();
        var notifier = scope.ServiceProvider.GetRequiredService<ITelemedicineRealtimeNotifier>();

        var activeSessions = await sessions.ListActiveSessionsAsync(ct);
        if (activeSessions.Count == 0)
        {
            return;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var session in activeSessions)
        {
            if (session.Status != TelemedicineSessionStatus.Active || session.StartedAtUtc is null)
            {
                continue;
            }

            var durationSeconds = Math.Max(0, (int)(now - session.StartedAtUtc.Value).TotalSeconds);
            await notifier.PublishDurationTickAsync(
                new TelemedicineDurationTickDto(session.AppointmentId, durationSeconds, now),
                ct);
        }
    }
}
