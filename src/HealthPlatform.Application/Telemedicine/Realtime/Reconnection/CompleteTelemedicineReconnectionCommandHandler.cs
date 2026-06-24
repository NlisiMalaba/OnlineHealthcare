using HealthPlatform.Application.Exceptions;
using HealthPlatform.Domain.Telemedicine;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Telemedicine.Realtime.Reconnection;

public sealed class CompleteTelemedicineReconnectionCommandHandler(
    TimeProvider timeProvider,
    ITelemedicineSessionParticipantService participantService,
    ITelemedicineSessionRepository telemedicineSessionRepository,
    ITelemedicineRealtimeNotifier realtimeNotifier,
    ILogger<CompleteTelemedicineReconnectionCommandHandler> logger)
    : IRequestHandler<CompleteTelemedicineReconnectionCommand, bool>
{
    public async Task<bool> Handle(CompleteTelemedicineReconnectionCommand request, CancellationToken ct)
    {
        var participant = await participantService.ResolveParticipantAsync(
            request.AppointmentId,
            requireActiveSession: false,
            ct);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var session = participant.Session;

        try
        {
            if (!session.TryCompleteReconnection(now, TelemedicinePolicies.ReconnectionGracePeriod))
            {
                return false;
            }
        }
        catch (TelemedicineReconnectionGraceExpiredException)
        {
            throw new DomainException(
                TelemedicineErrorCodes.ReconnectionGraceExpired,
                "Telemedicine session reconnection grace period has expired.");
        }

        await telemedicineSessionRepository.UpdateAsync(session, ct);

        await realtimeNotifier.PublishReconnectionSucceededAsync(
            new TelemedicineReconnectionSucceededDto(session.AppointmentId, now),
            ct);

        logger.LogInformation(
            "Completed telemedicine reconnection for appointment {AppointmentId}.",
            session.AppointmentId);

        return true;
    }
}
