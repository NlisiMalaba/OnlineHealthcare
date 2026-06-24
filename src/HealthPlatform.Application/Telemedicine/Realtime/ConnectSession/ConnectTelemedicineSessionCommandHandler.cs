using HealthPlatform.Application.Telemedicine.Realtime.Reconnection;
using MediatR;

namespace HealthPlatform.Application.Telemedicine.Realtime.ConnectSession;

public sealed class ConnectTelemedicineSessionCommandHandler(
    ISender sender,
    ITelemedicineSessionParticipantService participantService)
    : IRequestHandler<ConnectTelemedicineSessionCommand, ConnectTelemedicineSessionDto>
{
    public async Task<ConnectTelemedicineSessionDto> Handle(
        ConnectTelemedicineSessionCommand request,
        CancellationToken ct)
    {
        await participantService.ResolveParticipantAsync(
            request.AppointmentId,
            requireActiveSession: false,
            ct,
            allowWaitingSession: true);
        await sender.Send(new CompleteTelemedicineReconnectionCommand(request.AppointmentId), ct);

        return new ConnectTelemedicineSessionDto(
            request.AppointmentId,
            TelemedicineSessionGroupNames.ForAppointment(request.AppointmentId));
    }
}
