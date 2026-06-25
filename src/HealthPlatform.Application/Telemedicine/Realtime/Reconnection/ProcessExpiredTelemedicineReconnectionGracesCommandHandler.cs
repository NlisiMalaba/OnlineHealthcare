using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Telemedicine.Realtime.Reconnection;

public sealed class ProcessExpiredTelemedicineReconnectionGracesCommandHandler(
    TimeProvider timeProvider,
    ITelemedicineSessionRepository telemedicineSessionRepository,
    ITelemedicineRealtimeNotifier realtimeNotifier,
    ILogger<ProcessExpiredTelemedicineReconnectionGracesCommandHandler> logger)
    : IRequestHandler<ProcessExpiredTelemedicineReconnectionGracesCommand, int>
{
    public async Task<int> Handle(ProcessExpiredTelemedicineReconnectionGracesCommand request, CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var candidates = await telemedicineSessionRepository.ListSessionsWithPendingReconnectionGraceAsync(
            now,
            TelemedicinePolicies.ReconnectionGracePeriod,
            ct);

        var expiredCount = 0;

        foreach (var session in candidates)
        {
            if (!session.ExpireReconnectionGraceIfDue(now, TelemedicinePolicies.ReconnectionGracePeriod))
            {
                continue;
            }

            await telemedicineSessionRepository.UpdateAsync(session, ct);

            await realtimeNotifier.PublishReconnectionPromptRequiredAsync(
                new TelemedicineReconnectionPromptRequiredDto(session.AppointmentId, now),
                ct);

            expiredCount++;

            logger.LogInformation(
                "Telemedicine reconnection grace expired for appointment {AppointmentId}.",
                session.AppointmentId);
        }

        return expiredCount;
    }
}
