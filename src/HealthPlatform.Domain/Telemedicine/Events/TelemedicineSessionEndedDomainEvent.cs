using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Telemedicine.Events;

public sealed record TelemedicineSessionEndedDomainEvent(
    Guid SessionId,
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    TelemedicineSessionMode Mode,
    int DurationSeconds,
    DateTime StartedAtUtc,
    DateTime EndedAtUtc,
    bool RecordingEnabled) : DomainEvent;
