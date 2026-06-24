namespace HealthPlatform.Application.Telemedicine.Realtime;

public interface ITelemedicineSessionParticipantService
{
    Task<TelemedicineSessionParticipantContext> ResolveParticipantAsync(
        Guid appointmentId,
        bool requireActiveSession,
        CancellationToken ct);
}
