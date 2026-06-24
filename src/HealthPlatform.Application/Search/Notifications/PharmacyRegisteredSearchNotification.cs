using MediatR;

namespace HealthPlatform.Application.Search.Notifications;

public sealed record PharmacyRegisteredSearchNotification(
    Guid PharmacyId,
    DateTime OccurredAtUtc) : INotification;
