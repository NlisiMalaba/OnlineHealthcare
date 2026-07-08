using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Referrals;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job for appointment and other scheduled reminders.
/// </summary>
public sealed class ScheduledRemindersJob(
    IAppointmentReminderDispatcher appointmentReminderDispatcher,
    IReferralTimeoutReminderDispatcher referralTimeoutReminderDispatcher,
    ILogger<ScheduledRemindersJob> logger)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var appointmentDispatched = await appointmentReminderDispatcher.DispatchDueRemindersAsync(ct);
        var referralDispatched = await referralTimeoutReminderDispatcher.DispatchDueRemindersAsync(ct);
        var totalDispatched = appointmentDispatched + referralDispatched;
        if (totalDispatched > 0)
        {
            logger.LogInformation(
                "Scheduled reminders dispatched {AppointmentCount} appointment and {ReferralCount} referral timeout reminder(s).",
                appointmentDispatched,
                referralDispatched);
        }
        else
        {
            logger.LogDebug("Scheduled reminders tick — no appointment or referral timeout reminders due.");
        }
    }

    public void Run()
    {
        RunAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}
