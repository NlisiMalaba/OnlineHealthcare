using MediatR;

namespace HealthPlatform.Application.Identity.Notifications;

public sealed record DoctorLicenseRejectedNotification(
    Guid DoctorId,
    Guid UserId,
    string FullName,
    string Reason,
    DateTime OccurredAtUtc) : INotification;
