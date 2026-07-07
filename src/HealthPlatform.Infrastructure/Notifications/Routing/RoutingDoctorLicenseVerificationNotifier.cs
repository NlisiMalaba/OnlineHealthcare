using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Notifications;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingDoctorLicenseVerificationNotifier(INotificationDispatcher dispatcher)
    : IDoctorLicenseVerificationNotifier
{
    public Task NotifyLicenseVerifiedAsync(
        Guid userId,
        Guid doctorId,
        string fullName,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            userId,
            NotificationRecipientType.Doctor,
            NotificationEventTypes.DoctorLicenseVerified,
            NotificationCriticality.Standard,
            "License verified",
            "Your medical license has been verified.",
            new Dictionary<string, string>
            {
                ["doctor_id"] = doctorId.ToString()
            },
            ct);

    public Task NotifyLicenseRejectedAsync(
        Guid userId,
        Guid doctorId,
        string fullName,
        string reason,
        CancellationToken ct) =>
        NotificationRoutingSupport.DispatchToUserAsync(
            dispatcher,
            userId,
            NotificationRecipientType.Doctor,
            NotificationEventTypes.DoctorLicenseRejected,
            NotificationCriticality.Standard,
            "License verification rejected",
            "Your medical license verification was rejected.",
            new Dictionary<string, string>
            {
                ["doctor_id"] = doctorId.ToString()
            },
            ct);
}
