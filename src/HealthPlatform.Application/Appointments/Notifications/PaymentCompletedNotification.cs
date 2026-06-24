using MediatR;

namespace HealthPlatform.Application.Appointments.Notifications;

public sealed record PaymentCompletedNotification(
    Guid AppointmentId,
    Guid PaymentId,
    DateTime OccurredAtUtc) : INotification;
