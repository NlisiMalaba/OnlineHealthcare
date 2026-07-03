using FsCheck;
using HealthPlatform.Application.Notifications;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record NotificationDispatchCase(
    IReadOnlyList<NotificationChannel> Channels,
    bool PushSucceeds,
    bool SmsSucceeds,
    bool EmailSucceeds,
    string EventType,
    NotificationRecipientType RecipientType);

public static class NotificationLogArbitraries
{
    private static readonly string[] EventTypes =
    [
        NotificationEventTypes.AppointmentConfirmed,
        NotificationEventTypes.AppointmentReminder,
        NotificationEventTypes.PrescriptionIssued,
        NotificationEventTypes.MedicationDoseReminder,
        NotificationEventTypes.OrderStatusChanged,
        NotificationEventTypes.EmergencyAlert
    ];

    private static readonly NotificationRecipientType[] RecipientTypes =
    [
        NotificationRecipientType.Patient,
        NotificationRecipientType.Doctor,
        NotificationRecipientType.Pharmacy,
        NotificationRecipientType.NextOfKin
    ];

    public static Arbitrary<NotificationDispatchCase> NotificationDispatchCase() =>
        (from channelMask in Gen.Choose(1, 7)
         from pushSucceeds in Arb.Generate<bool>()
         from smsSucceeds in Arb.Generate<bool>()
         from emailSucceeds in Arb.Generate<bool>()
         from eventType in Gen.Elements(EventTypes)
         from recipientType in Gen.Elements(RecipientTypes)
         select new NotificationDispatchCase(
             DecodeChannels(channelMask),
             pushSucceeds,
             smsSucceeds,
             emailSucceeds,
             eventType,
             recipientType))
        .ToArbitrary();

    private static IReadOnlyList<NotificationChannel> DecodeChannels(int channelMask)
    {
        var channels = new List<NotificationChannel>(3);
        if ((channelMask & 1) != 0)
        {
            channels.Add(NotificationChannel.Push);
        }

        if ((channelMask & 2) != 0)
        {
            channels.Add(NotificationChannel.Email);
        }

        if ((channelMask & 4) != 0)
        {
            channels.Add(NotificationChannel.Sms);
        }

        return channels;
    }
}
