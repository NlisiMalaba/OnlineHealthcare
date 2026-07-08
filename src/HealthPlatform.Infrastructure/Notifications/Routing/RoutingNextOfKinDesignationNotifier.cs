using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingNextOfKinDesignationNotifier(INotificationDispatcher dispatcher)
    : INextOfKinDesignationNotifier
{
    public Task NotifyDesignatedAsync(
        NextOfKinContactDto contact,
        string patientFullName,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToContactAsync(
            dispatcher,
            NotificationRecipientType.NextOfKin,
            NotificationEventTypes.NextOfKinDesignated,
            NotificationCriticality.Standard,
            "Next-of-kin designation",
            "You have been designated as a next-of-kin contact.",
            new NotificationContactOverride(contact.Email, contact.PhoneNumber),
            new Dictionary<string, string>
            {
                ["contact_id"] = contact.Id.ToString(),
                ["patient_id"] = contact.PatientId.ToString()
            },
            ct);
}
