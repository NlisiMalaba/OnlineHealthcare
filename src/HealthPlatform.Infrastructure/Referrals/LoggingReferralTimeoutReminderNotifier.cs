using HealthPlatform.Application.Referrals;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Referrals;

public sealed class LoggingReferralTimeoutReminderNotifier(
    ILogger<LoggingReferralTimeoutReminderNotifier> logger)
    : IReferralTimeoutReminderNotifier
{
    public Task NotifyReferralTimeoutReminderAsync(
        Guid receivingDoctorUserId,
        Guid referralId,
        Guid patientId,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Referral timeout reminder requested for referral {ReferralId} and receiving doctor user {ReceivingDoctorUserId}.",
            referralId,
            receivingDoctorUserId);
        return Task.CompletedTask;
    }
}
