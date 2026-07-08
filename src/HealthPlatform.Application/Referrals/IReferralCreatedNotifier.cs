namespace HealthPlatform.Application.Referrals;

public interface IReferralCreatedNotifier
{
    Task NotifyReferralCreatedAsync(
        Guid patientUserId,
        Guid? receivingDoctorUserId,
        Guid referralId,
        Guid referringDoctorId,
        string reason,
        CancellationToken ct);
}
