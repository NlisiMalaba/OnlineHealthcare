using MediatR;

namespace HealthPlatform.Application.Appointments.Notifications;

public sealed record AppointmentRescheduledNotification(
    Guid AppointmentId,
    Guid PatientId,
    Guid DoctorId,
    DateTime PreviousScheduledAtUtc,
    DateTime NewScheduledAtUtc,
    DateTime OccurredAtUtc) : INotification;
