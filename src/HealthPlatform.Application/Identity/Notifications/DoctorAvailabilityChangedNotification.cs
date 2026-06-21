using MediatR;

namespace HealthPlatform.Application.Identity.Notifications;

public sealed record DoctorAvailabilityChangedNotification(
    Guid DoctorId,
    DateTime OccurredAtUtc) : INotification;
