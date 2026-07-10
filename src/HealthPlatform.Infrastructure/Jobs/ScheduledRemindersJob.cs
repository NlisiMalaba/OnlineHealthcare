using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Referrals;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Application.Vaccinations;
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
    IVaccinationReminderDispatcher vaccinationReminderDispatcher,
    ILogger<ScheduledRemindersJob> logger)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var appointmentDispatched = await appointmentReminderDispatcher.DispatchDueRemindersAsync(ct);
        var referralDispatched = await referralTimeoutReminderDispatcher.DispatchDueRemindersAsync(ct);
        var antenatalDispatched = await antenatalCheckupReminderDispatcher.DispatchDueRemindersAsync(ct);
        var fetalMonitoringDispatched = await fetalMonitoringReminderDispatcher.DispatchDueRemindersAsync(ct);
        var vaccinationDispatched = await vaccinationReminderDispatcher.DispatchDueRemindersAsync(ct);
        var totalDispatched = appointmentDispatched
            + referralDispatched
            + antenatalDispatched
            + fetalMonitoringDispatched
            + vaccinationDispatched;
        if (totalDispatched > 0)
        {
            logger.LogInformation(
                "Scheduled reminders dispatched {AppointmentCount} appointment, {ReferralCount} referral timeout, {AntenatalCount} antenatal, {FetalMonitoringCount} fetal monitoring, and {VaccinationCount} vaccination reminder(s).",
                appointmentDispatched,
                referralDispatched,
                antenatalDispatched,
                fetalMonitoringDispatched,
                vaccinationDispatched);
        }
        else
        {
            logger.LogDebug(
                "Scheduled reminders tick — no appointment, referral timeout, antenatal, fetal monitoring, or vaccination reminders due.");
        }
    }

    public void Run()
    {
        RunAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}
