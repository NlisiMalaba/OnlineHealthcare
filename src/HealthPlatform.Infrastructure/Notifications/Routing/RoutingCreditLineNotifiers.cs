using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Payments.CreditLine;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingCreditBalanceWarningNotifier(INotificationDispatcher dispatcher)
    : ICreditBalanceWarningNotifier
{
    public Task NotifyBalanceWarningAsync(
        Guid patientUserId,
        Guid patientId,
        long outstandingBalanceMinorUnits,
        long creditLimitMinorUnits,
        string currency,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.CreditBalanceWarning,
            NotificationCriticality.Standard,
            "Credit balance warning",
            "Your credit balance is approaching the limit.",
            new Dictionary<string, string>
            {
                ["patient_id"] = patientId.ToString(),
                ["currency"] = currency
            },
            ct);
}

public sealed class RoutingCreditRepaymentReminderNotifier(INotificationDispatcher dispatcher)
    : ICreditRepaymentReminderNotifier
{
    public Task NotifyRepaymentReminderAsync(
        Guid patientUserId,
        Guid patientId,
        Guid transactionId,
        long amountChargedMinorUnits,
        long outstandingBalanceMinorUnits,
        string currency,
        DateTime repaymentDueAtUtc,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.CreditRepaymentReminder,
            NotificationCriticality.Standard,
            "Credit repayment reminder",
            "Your credit repayment is due soon.",
            new Dictionary<string, string>
            {
                ["patient_id"] = patientId.ToString(),
                ["transaction_id"] = transactionId.ToString(),
                ["currency"] = currency,
                ["repayment_due_at_utc"] = repaymentDueAtUtc.ToString("O")
            },
            ct);
}
