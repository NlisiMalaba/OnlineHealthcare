namespace HealthPlatform.Application.Payments.CreditLine;

public interface ICreditRepaymentReminderNotifier
{
    Task NotifyRepaymentReminderAsync(
        Guid patientUserId,
        Guid patientId,
        Guid transactionId,
        long amountChargedMinorUnits,
        long outstandingBalanceMinorUnits,
        string currency,
        DateTime repaymentDueAtUtc,
        CancellationToken ct);
}

public interface ICreditBalanceWarningNotifier
{
    Task NotifyBalanceWarningAsync(
        Guid patientUserId,
        Guid patientId,
        long outstandingBalanceMinorUnits,
        long creditLimitMinorUnits,
        string currency,
        CancellationToken ct);
}
