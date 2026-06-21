using MediatR;

namespace HealthPlatform.Application.Identity.Notifications;

public sealed record DoctorLicenseVerifiedNotification(
    Guid DoctorId,
    Guid UserId,
    string FullName,
    DateTime OccurredAtUtc) : INotification;
