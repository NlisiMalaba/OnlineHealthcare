using HealthPlatform.Application.Wellness;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job that dispatches push reminders when scheduled medication dose times are reached.
/// </summary>
public sealed class MedicationDoseReminderJob(
    IMedicationDoseReminderDispatcher reminderDispatcher,
    ILogger<MedicationDoseReminderJob> logger)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var dispatched = await reminderDispatcher.DispatchDueRemindersAsync(ct);
        if (dispatched > 0)
        {
            logger.LogInformation("Medication dose reminders dispatched {Count} reminder(s).", dispatched);
        }
        else
        {
            logger.LogDebug("Medication dose reminder tick — no reminders due.");
        }
    }

    public void Run()
    {
        RunAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}
