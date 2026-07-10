using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Referrals;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job for appointment and other scheduled reminders.
/// </summary>
public sealed class ScheduledRemindersJob(
    IAppointmentReminderDispatcher appointmentReminderDispatcher,
    IReferralTimeoutReminderDispatcher referralTimeoutReminderDispatcher,
    IAntenatalCheckupReminderDispatcher antenatalCheckupReminderDispatcher,
    IFetalMonitoringReminderDispatcher fetalMonitoringReminderDispatcher,
    ILogger<ScheduledRemindersJob> logger)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var appointmentDispatched = await appointmentReminderDispatcher.DispatchDueRemindersAsync(ct);
        var referralDispatched = await referralTimeoutReminderDispatcher.DispatchDueRemindersAsync(ct);
        var antenatalDispatched = await antenatalCheckupReminderDispatcher.DispatchDueRemindersAsync(ct);
        var fetalMonitoringDispatched = await fetalMonitoringReminderDispatcher.DispatchDueRemindersAsync(ct);
        var totalDispatched = appointmentDispatched
            + referralDispatched
            + antenatalDispatched
            + fetalMonitoringDispatched;
        if (totalDispatched > 0)
        {
            logger.LogInformation(
                "Scheduled reminders dispatched {AppointmentCount} appointment, {ReferralCount} referral timeout, {AntenatalCount} antenatal, and {FetalMonitoringCount} fetal monitoring reminder(s).",
                appointmentDispatched,
                referralDispatched,
                antenatalDispatched,
                fetalMonitoringDispatched);
        }
        else
        {
            logger.LogDebug(
                "Scheduled reminders tick — no appointment, referral timeout, antenatal, or fetal monitoring reminders due.");
        }
    }

    public void Run()
    {
        RunAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}
