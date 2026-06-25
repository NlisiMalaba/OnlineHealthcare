using HealthPlatform.Domain.Telemedicine;

namespace HealthPlatform.Application.Telemedicine.Realtime;

public sealed record TelemedicineSessionParticipantContext(
    Guid AppointmentId,
    Guid SessionId,
    TelemedicineSessionParticipantRole Role,
    TelemedicineSession Session);
