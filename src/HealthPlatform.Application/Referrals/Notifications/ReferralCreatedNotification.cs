using MediatR;

namespace HealthPlatform.Application.Referrals.Notifications;

public sealed record ReferralCreatedNotification(
    Guid ReferralId,
    Guid PatientId,
    Guid ReferringDoctorId,
    Guid? ReceivingDoctorId,
    string Reason,
    DateTime PatientConsentAtUtc,
    DateTime CreatedAtUtc,
    DateTime OccurredAtUtc) : INotification;
