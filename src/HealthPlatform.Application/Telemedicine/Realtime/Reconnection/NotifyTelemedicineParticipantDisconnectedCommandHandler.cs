using HealthPlatform.Domain.Telemedicine;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Telemedicine.Realtime.Reconnection;

public sealed class NotifyTelemedicineParticipantDisconnectedCommandHandler(
    TimeProvider timeProvider,
    ITelemedicineSessionParticipantService participantService,
    ITelemedicineSessionRepository telemedicineSessionRepository,
    ITelemedicineRealtimeNotifier realtimeNotifier,
    ILogger<NotifyTelemedicineParticipantDisconnectedCommandHandler> logger)
    : IRequestHandler<NotifyTelemedicineParticipantDisconnectedCommand>
{
    public async Task Handle(NotifyTelemedicineParticipantDisconnectedCommand request, CancellationToken ct)
    {
        var participant = await participantService.ResolveParticipantAsync(
            request.AppointmentId,
            requireActiveSession: true,
            ct);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var session = participant.Session;

        if (!session.BeginReconnectionGrace(now))
        {
            return;
        }

        await telemedicineSessionRepository.UpdateAsync(session, ct);

        var deadline = session.GetReconnectionDeadlineUtc(TelemedicinePolicies.ReconnectionGracePeriod)
            ?? now.Add(TelemedicinePolicies.ReconnectionGracePeriod);

        var remainingSeconds = Math.Max(
            0,
            (int)Math.Ceiling((deadline - now).TotalSeconds));

        await realtimeNotifier.PublishReconnectionAttemptingAsync(
            new TelemedicineReconnectionAttemptingDto(
                session.AppointmentId,
                now,
                deadline,
                remainingSeconds),
            ct);

        logger.LogInformation(
            "Started telemedicine reconnection grace for appointment {AppointmentId}.",
            session.AppointmentId);
    }
}
