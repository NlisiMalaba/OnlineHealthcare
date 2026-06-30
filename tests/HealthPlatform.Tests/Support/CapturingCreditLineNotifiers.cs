using HealthPlatform.Application.Payments.CreditLine;

namespace HealthPlatform.Tests.Support;

public sealed class CapturingCreditBalanceWarningNotifier : ICreditBalanceWarningNotifier
{
    public List<(
        Guid PatientUserId,
        Guid PatientId,
        long OutstandingBalanceMinorUnits,
        long CreditLimitMinorUnits,
        string Currency)> Notifications { get; } = [];

    public Task NotifyBalanceWarningAsync(
        Guid patientUserId,
        Guid patientId,
        long outstandingBalanceMinorUnits,
        long creditLimitMinorUnits,
        string currency,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        Notifications.Add((
            patientUserId,
            patientId,
            outstandingBalanceMinorUnits,
            creditLimitMinorUnits,
            currency));
        return Task.CompletedTask;
    }
}

public sealed class CapturingCreditRepaymentReminderNotifier : ICreditRepaymentReminderNotifier
{
    public List<(
        Guid PatientUserId,
        Guid PatientId,
        Guid TransactionId,
        long AmountChargedMinorUnits,
        long OutstandingBalanceMinorUnits,
        string Currency,
        DateTime RepaymentDueAtUtc)> Notifications { get; } = [];

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
        ct.ThrowIfCancellationRequested();
        Notifications.Add((
            patientUserId,
            patientId,
            transactionId,
            amountChargedMinorUnits,
            outstandingBalanceMinorUnits,
            currency,
            repaymentDueAtUtc));
        return Task.CompletedTask;
    }
}
