namespace HealthPlatform.Application.Referrals;

public interface IReferralTimeoutReminderDispatcher
{
    Task<int> DispatchDueRemindersAsync(CancellationToken ct);
}
