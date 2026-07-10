namespace HealthPlatform.Application.Referrals;

public interface IReferralStatusChangedNotifier
{
    Task NotifyReferralStatusChangedAsync(
        Guid patientUserId,
        Guid referringDoctorUserId,
        Guid referralId,
        string status,
        string? reason,
        CancellationToken ct);
}
