using MediatR;

namespace HealthPlatform.Application.Identity.Notifications;

public sealed record PatientRegisteredNotification(
    Guid PatientId,
    DateTime OccurredAtUtc) : INotification;
