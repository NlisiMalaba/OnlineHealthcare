using System.Text.Json;
using HealthPlatform.Domain.Notifications;

namespace HealthPlatform.Application.Notifications;

public static class NotificationLogRecipientIdResolver
{
    public static Guid Resolve(NotificationDispatchRequest request)
    {
        if (request.UserId.HasValue)
        {
            return request.UserId.Value;
        }

        if (request.Metadata is not null
            && request.Metadata.TryGetValue("contact_id", out var contactId)
            && Guid.TryParse(contactId, out var parsedContactId))
        {
            return parsedContactId;
        }

        throw new InvalidOperationException(
            "Notification log requires a recipient user id or contact_id metadata value.");
    }
}

public static class NotificationLogMappings
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static string ToRecipientTypeKey(NotificationRecipientType recipientType) =>
        recipientType switch
        {
            NotificationRecipientType.Patient => "patient",
            NotificationRecipientType.Doctor => "doctor",
            NotificationRecipientType.Pharmacy => "pharmacy",
            NotificationRecipientType.Admin => "admin",
            NotificationRecipientType.NextOfKin => "next_of_kin",
            _ => throw new ArgumentOutOfRangeException(nameof(recipientType), recipientType, null)
        };

    public static string ToChannelKey(NotificationChannel channel) =>
        NotificationPreferenceDefaults.ToChannelKey(channel);

    public static NotificationDeliveryStatus ToDeliveryStatus(bool succeeded) =>
        succeeded ? NotificationDeliveryStatus.Sent : NotificationDeliveryStatus.Failed;

    public static string SerializePayload(IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
        {
            return "{}";
        }

        return JsonSerializer.Serialize(metadata, SerializerOptions);
    }
}
