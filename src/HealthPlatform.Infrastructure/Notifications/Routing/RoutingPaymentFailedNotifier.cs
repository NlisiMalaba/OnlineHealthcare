using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Payments;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingPaymentFailedNotifier(INotificationDispatcher dispatcher)
    : IPaymentFailedNotifier
{
    public Task NotifyPaymentFailedAsync(
        Guid patientUserId,
        Guid patientId,
        Guid paymentId,
        Guid? appointmentId,
        Guid? medicationOrderId,
        string failureCode,
        string failureMessage,
        DateTime retentionExpiresAtUtc,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.PaymentFailed,
            NotificationCriticality.Standard,
            "Payment failed",
            "Your payment could not be completed.",
            new Dictionary<string, string>
            {
                ["payment_id"] = paymentId.ToString(),
                ["patient_id"] = patientId.ToString(),
                ["failure_code"] = failureCode,
                ["retention_expires_at_utc"] = retentionExpiresAtUtc.ToString("O")
            },
            ct);
}
