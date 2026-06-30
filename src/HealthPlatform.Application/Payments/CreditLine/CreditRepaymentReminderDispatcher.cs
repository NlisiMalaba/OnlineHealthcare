using HealthPlatform.Application.Identity;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Application.Payments.CreditLine;

public sealed class CreditRepaymentReminderDispatcher(
    ICreditLineTransactionRepository transactionRepository,
    IPatientRepository patientRepository,
    ICreditRepaymentReminderNotifier repaymentReminderNotifier,
    TimeProvider timeProvider,
    ILogger<CreditRepaymentReminderDispatcher> logger) : ICreditRepaymentReminderDispatcher
{
    private const int BatchSize = 50;
    private static readonly TimeSpan ReminderLeadTime = TimeSpan.FromHours(24);

    public async Task<int> DispatchDueRemindersAsync(CancellationToken ct)
    {
        var dueBeforeUtc = timeProvider.GetUtcNow().UtcDateTime.Add(ReminderLeadTime);
        var transactions = await transactionRepository.ListDueRepaymentRemindersAsync(dueBeforeUtc, BatchSize, ct);
        var dispatched = 0;

        foreach (var transaction in transactions)
        {
            var patient = await patientRepository.GetByIdAsync(transaction.PatientId, ct);
            if (patient is null)
            {
                logger.LogWarning(
                    "Skipping credit repayment reminder for missing patient {PatientId}.",
                    transaction.PatientId);
                continue;
            }

            await repaymentReminderNotifier.NotifyRepaymentReminderAsync(
                patient.UserId,
                patient.Id,
                transaction.Id,
                transaction.AmountMinorUnits,
                transaction.OutstandingBalanceAfterMinorUnits,
                transaction.Currency,
                transaction.RepaymentDueAtUtc,
                ct);

            transaction.MarkRepaymentReminderSent();
            await transactionRepository.UpdateAsync(transaction, ct);
            dispatched++;
        }

        if (dispatched > 0)
        {
            await transactionRepository.SaveChangesAsync(ct);
        }

        return dispatched;
    }
}
