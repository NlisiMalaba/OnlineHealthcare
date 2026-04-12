using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Jobs;

/// <summary>
/// Placeholder for scheduled reminders (instalments, referrals, etc.).
/// </summary>
public sealed class ScheduledRemindersJob(ILogger<ScheduledRemindersJob> logger)
{
    public void Run()
    {
        logger.LogInformation("Scheduled reminders tick — no reminders due (scaffold).");
    }
}
