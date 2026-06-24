using MediatR;

namespace HealthPlatform.Application.Telemedicine.Realtime.ConnectSession;

public sealed class ConnectTelemedicineSessionCommandHandler(
    ITelemedicineSessionParticipantService participantService)
    : IRequestHandler<ConnectTelemedicineSessionCommand, ConnectTelemedicineSessionDto>
{
    public async Task<ConnectTelemedicineSessionDto> Handle(
        ConnectTelemedicineSessionCommand request,
        CancellationToken ct)
    {
        await participantService.ResolveParticipantAsync(request.AppointmentId, requireActiveSession: false, ct);

        return new ConnectTelemedicineSessionDto(
            request.AppointmentId,
            TelemedicineSessionGroupNames.ForAppointment(request.AppointmentId));
    }
}
