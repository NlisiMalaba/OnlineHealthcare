using MediatR;

namespace HealthPlatform.Application.Identity.Notifications;

public sealed record DoctorRegisteredNotification(
    Guid DoctorId,
    string LicenseNumber,
    string FullName,
    DateTime OccurredAtUtc) : INotification;
