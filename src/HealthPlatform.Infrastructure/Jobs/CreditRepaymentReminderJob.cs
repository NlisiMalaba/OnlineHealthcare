using HealthPlatform.Application.Payments.CreditLine;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Jobs;

public sealed class CreditRepaymentReminderJob(
    ICreditRepaymentReminderDispatcher reminderDispatcher,
    ILogger<CreditRepaymentReminderJob> logger)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        var dispatched = await reminderDispatcher.DispatchDueRemindersAsync(ct);
        if (dispatched > 0)
        {
            logger.LogInformation("Credit repayment reminders dispatched {Count} reminder(s).", dispatched);
        }
        else
        {
            logger.LogDebug("Credit repayment reminder tick — no reminders due.");
        }
    }

    public void Run()
    {
        RunAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}
