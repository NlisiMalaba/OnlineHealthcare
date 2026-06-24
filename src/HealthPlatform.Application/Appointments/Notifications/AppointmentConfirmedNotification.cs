using MediatR;

namespace HealthPlatform.Application.Appointments.Notifications;

public sealed record AppointmentConfirmedNotification(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    DateTime ScheduledAtUtc,
    DateTime ConfirmedAtUtc,
    DateTime OccurredAtUtc) : INotification;
