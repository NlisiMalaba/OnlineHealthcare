using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Infrastructure.Notifications.Routing;

public sealed class RoutingNextOfKinChannelDeliveryGateway(INotificationDispatcher dispatcher)
    : INextOfKinChannelDeliveryGateway
{
    public async Task<bool> TryDeliverEmergencyAlertAsync(
        EmergencyAlertChannelDeliveryRequest request,
        CancellationToken ct)
    {
        var channel = request.Channel == NextOfKinNotificationChannel.Sms
            ? NotificationChannel.Sms
            : NotificationChannel.Push;

        var result = await NotificationRoutingSupport.DispatchToContactAsync(
            dispatcher,
            NotificationRecipientType.NextOfKin,
            NotificationEventTypes.EmergencyAlert,
            NotificationCriticality.Critical,
            "Emergency alert",
            "An emergency alert retry delivery is being attempted.",
            new NotificationContactOverride(request.Contact.Email, request.Contact.PhoneNumber),
            new Dictionary<string, string>
            {
                ["emergency_alert_id"] = request.EmergencyAlertId.ToString(),
                ["patient_id"] = request.PatientId.ToString(),
                ["contact_id"] = request.Contact.Id.ToString()
            },
            ct,
            [channel]);

        var channelResult = result.ChannelResults.FirstOrDefault(entry => entry.Channel == channel);
        return channelResult?.Succeeded == true;
    }
}
