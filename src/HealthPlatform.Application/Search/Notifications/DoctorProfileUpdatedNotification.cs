using MediatR;

namespace HealthPlatform.Application.Search.Notifications;

public sealed record DoctorProfileUpdatedNotification(
    Guid DoctorId,
    DateTime OccurredAtUtc) : INotification;
