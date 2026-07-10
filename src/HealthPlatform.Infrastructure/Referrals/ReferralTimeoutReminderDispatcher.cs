using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Referrals;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Referrals;

public sealed class ReferralTimeoutReminderDispatcher(
    TimeProvider timeProvider,
    IReferralRepository referralRepository,
    IDoctorRepository doctorRepository,
    IReferralTimeoutReminderNotifier notifier,
    ILogger<ReferralTimeoutReminderDispatcher> logger) : IReferralTimeoutReminderDispatcher
{
    public async Task<int> DispatchDueRemindersAsync(CancellationToken ct)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var dueReferrals = await referralRepository.ListPendingForTimeoutReminderAsync(
            now,
            ReferralPolicies.TimeoutReminderThreshold,
            ct);

        if (dueReferrals.Count == 0)
        {
            return 0;
        }

        var dispatched = 0;
        foreach (var referral in dueReferrals)
        {
            ct.ThrowIfCancellationRequested();

            if (!referral.ReceivingDoctorId.HasValue)
            {
                logger.LogWarning(
                    "Skipping referral timeout reminder for referral {ReferralId}; receiving doctor is missing.",
                    referral.Id);
                continue;
            }

            var receivingDoctor = await doctorRepository.GetByIdAsync(referral.ReceivingDoctorId.Value, ct);
            if (receivingDoctor is null)
            {
                logger.LogWarning(
                    "Skipping referral timeout reminder for referral {ReferralId}; receiving doctor {ReceivingDoctorId} not found.",
                    referral.Id,
                    referral.ReceivingDoctorId.Value);
                continue;
            }

            await notifier.NotifyReferralTimeoutReminderAsync(
                receivingDoctor.UserId,
                referral.Id,
                referral.PatientId,
                ct);

            if (!referral.MarkTimeoutReminderSent(now))
            {
                continue;
            }

            await referralRepository.UpdateAsync(referral, ct);
            dispatched++;
        }

        return dispatched;
    }
}
