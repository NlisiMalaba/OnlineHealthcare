namespace HealthPlatform.Application.Referrals;

public interface IReferralTimeoutReminderNotifier
{
    Task NotifyReferralTimeoutReminderAsync(
        Guid receivingDoctorUserId,
        Guid referralId,
        Guid patientId,
        CancellationToken ct);
}
