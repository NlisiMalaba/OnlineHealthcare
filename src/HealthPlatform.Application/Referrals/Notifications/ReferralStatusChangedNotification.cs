using MediatR;

namespace HealthPlatform.Application.Referrals.Notifications;

public sealed record ReferralStatusChangedNotification(
    Guid ReferralId,
    Guid PatientId,
    Guid ReferringDoctorId,
    Guid? ReceivingDoctorId,
    string Status,
    string? Reason,
    DateTime RespondedAtUtc,
    DateTime OccurredAtUtc) : INotification;
