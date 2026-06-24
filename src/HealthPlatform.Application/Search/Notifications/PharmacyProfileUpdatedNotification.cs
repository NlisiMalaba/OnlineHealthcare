using MediatR;

namespace HealthPlatform.Application.Search.Notifications;

public sealed record PharmacyProfileUpdatedNotification(
    Guid PharmacyId,
    DateTime OccurredAtUtc) : INotification;
