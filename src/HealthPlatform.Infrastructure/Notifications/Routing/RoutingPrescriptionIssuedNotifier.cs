using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Prescriptions;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingPrescriptionIssuedNotifier(INotificationDispatcher dispatcher)
    : IPrescriptionIssuedNotifier
{
    public Task NotifyPrescriptionIssuedAsync(
        Guid patientUserId,
        Guid prescriptionId,
        DateTime issuedAtUtc,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            patientUserId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.PrescriptionIssued,
            NotificationCriticality.Standard,
            "Prescription issued",
            "A new prescription is available in your health record.",
            new Dictionary<string, string>
            {
                ["prescription_id"] = prescriptionId.ToString(),
                ["issued_at_utc"] = issuedAtUtc.ToString("O")
            },
            ct);
}
