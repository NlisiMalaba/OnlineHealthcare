using HealthPlatform.Application.Payments.CreditLine;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Payments.CreditLine;

public sealed class LoggingCreditRepaymentReminderNotifier(
    ILogger<LoggingCreditRepaymentReminderNotifier> logger) : ICreditRepaymentReminderNotifier
{
    public Task NotifyRepaymentReminderAsync(
        Guid patientUserId,
        Guid patientId,
        Guid transactionId,
        long amountChargedMinorUnits,
        long outstandingBalanceMinorUnits,
        string currency,
        DateTime repaymentDueAtUtc,
        CancellationToken ct)
    {
        logger.LogInformation(
            "Credit repayment reminder for patient {PatientId}, transaction {TransactionId}, outstanding {Outstanding} {Currency}, due {DueAtUtc}.",
            patientId,
            transactionId,
            outstandingBalanceMinorUnits,
            currency,
            repaymentDueAtUtc);

        return Task.CompletedTask;
    }
}
