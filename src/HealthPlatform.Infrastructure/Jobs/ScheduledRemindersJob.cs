using HealthPlatform.Application.Appointments;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job for appointment and other scheduled reminders.
/// </summary>
public sealed class ScheduledRemindersJob(
    IAppointmentReminderDispatcher appointmentReminderDispatcher,
    ILogger<ScheduledRemindersJob> logger)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var dispatched = await appointmentReminderDispatcher.DispatchDueRemindersAsync(ct);
        if (dispatched > 0)
        {
            logger.LogInformation("Scheduled reminders dispatched {Count} appointment reminder(s).", dispatched);
        }
        else
        {
            logger.LogDebug("Scheduled reminders tick — no appointment reminders due.");
        }
    }

    public void Run()
    {
        RunAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}
