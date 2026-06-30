using HealthPlatform.Application.Payments.Instalments;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Jobs;

public sealed class InstalmentDueReminderJob(
    IInstalmentDueReminderDispatcher reminderDispatcher,
    ILogger<InstalmentDueReminderJob> logger)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var dispatched = await reminderDispatcher.DispatchDueRemindersAsync(ct);
        if (dispatched > 0)
        {
            logger.LogInformation("Instalment due reminders dispatched {Count} reminder(s).", dispatched);
        }
        else
        {
            logger.LogDebug("Instalment due reminder tick — no reminders due.");
        }
    }

    public void Run()
    {
        RunAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}

public sealed class InstalmentMissedPaymentJob(
    IInstalmentMissedPaymentProcessor missedPaymentProcessor,
    ILogger<InstalmentMissedPaymentJob> logger)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var processed = await missedPaymentProcessor.ProcessMissedPaymentsAsync(ct);
        if (processed > 0)
        {
            logger.LogInformation("Instalment missed payments processed {Count} payment(s).", processed);
        }
        else
        {
            logger.LogDebug("Instalment missed payment tick — no missed payments found.");
        }
    }

    public void Run()
    {
        RunAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}
