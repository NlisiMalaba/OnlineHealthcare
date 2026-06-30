using HealthPlatform.Application.Payments.Instalments;
using Microsoft.Extensions.Logging;

namespace HealthPlatform.Infrastructure.Payments.Instalments;

public sealed class LoggingInstalmentDueReminderNotifier(
    ILogger<LoggingInstalmentDueReminderNotifier> logger) : IInstalmentDueReminderNotifier
{
    public Task NotifyDueReminderAsync(
        Guid patientUserId,
        Guid patientId,
        Guid instalmentPlanId,
        Guid instalmentPaymentId,
        int sequenceNumber,
        long amountMinorUnits,
        string currency,
        DateOnly dueDate,
        CancellationToken ct)
    {
        logger.LogInformation(
            "Instalment due reminder for patient {PatientId}, plan {PlanId}, payment {PaymentId}, sequence {Sequence}, amount {Amount} {Currency}, due {DueDate}.",
            patientId,
            instalmentPlanId,
            instalmentPaymentId,
            sequenceNumber,
            amountMinorUnits,
            currency,
            dueDate);

        return Task.CompletedTask;
    }
}

public sealed class LoggingInstalmentMissedPaymentNotifier(
    ILogger<LoggingInstalmentMissedPaymentNotifier> logger) : IInstalmentMissedPaymentNotifier
{
    public Task NotifyMissedPaymentAsync(
        Guid patientUserId,
        Guid patientId,
        Guid instalmentPlanId,
        Guid instalmentPaymentId,
        int sequenceNumber,
        long amountMinorUnits,
        long lateFeeMinorUnits,
        string currency,
        DateOnly dueDate,
        CancellationToken ct)
    {
        logger.LogWarning(
            "Missed instalment for patient {PatientId}, plan {PlanId}, payment {PaymentId}, sequence {Sequence}, amount {Amount} {Currency}, late fee {LateFee}, due {DueDate}.",
            patientId,
            instalmentPlanId,
            instalmentPaymentId,
            sequenceNumber,
            amountMinorUnits,
            currency,
            lateFeeMinorUnits,
            dueDate);

        return Task.CompletedTask;
    }
}
