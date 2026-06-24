using HealthPlatform.Domain.Telemedicine;
using MediatR;

namespace HealthPlatform.Application.Telemedicine.Notifications;

public sealed record TelemedicineSessionEndedNotification(
    Guid SessionId,
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    TelemedicineSessionMode Mode,
    int DurationSeconds,
    DateTime StartedAtUtc,
    DateTime EndedAtUtc,
    bool RecordingEnabled,
    DateTime OccurredAtUtc) : INotification;
