using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Payments.Instalments;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingInstalmentDueReminderNotifier(INotificationDispatcher dispatcher)
    : IInstalmentDueReminderNotifier
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
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.InstalmentDueReminder,
            NotificationCriticality.Standard,
            "Instalment due soon",
            "An instalment payment is due soon.",
            new Dictionary<string, string>
            {
                ["patient_id"] = patientId.ToString(),
                ["instalment_plan_id"] = instalmentPlanId.ToString(),
                ["instalment_payment_id"] = instalmentPaymentId.ToString(),
                ["sequence_number"] = sequenceNumber.ToString(),
                ["due_date"] = dueDate.ToString("O")
            },
            ct);
}

public sealed class RoutingInstalmentMissedPaymentNotifier(INotificationDispatcher dispatcher)
    : IInstalmentMissedPaymentNotifier
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
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.InstalmentMissedPayment,
            NotificationCriticality.Standard,
            "Instalment payment missed",
            "An instalment payment was missed.",
            new Dictionary<string, string>
            {
                ["patient_id"] = patientId.ToString(),
                ["instalment_plan_id"] = instalmentPlanId.ToString(),
                ["instalment_payment_id"] = instalmentPaymentId.ToString(),
                ["sequence_number"] = sequenceNumber.ToString(),
                ["due_date"] = dueDate.ToString("O")
            },
            ct);
}
