using FsCheck;
using HealthPlatform.Application.Notifications;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record CriticalNotificationSmsFallbackCase(
    string EventType,
    bool PushSucceeds,
    bool IncludeSmsChannel,
    bool SmsSucceeds);

public static class CriticalNotificationSmsFallbackArbitraries
{
    private static readonly string[] CriticalEventTypes =
    [
        NotificationEventTypes.MedicationDoseReminder,
        NotificationEventTypes.EmergencyAlert
    ];

    private static readonly string[] NonCriticalEventTypes =
    [
        NotificationEventTypes.AppointmentConfirmed,
        NotificationEventTypes.AppointmentReminder,
        NotificationEventTypes.PrescriptionIssued
    ];

    public static Arbitrary<CriticalNotificationSmsFallbackCase> CriticalNotificationSmsFallbackCase() =>
        (from isCriticalEvent in Arb.Generate<bool>()
         from eventType in isCriticalEvent
             ? Gen.Elements(CriticalEventTypes)
             : Gen.Elements(NonCriticalEventTypes)
         from pushSucceeds in Arb.Generate<bool>()
         from includeSmsChannel in Arb.Generate<bool>()
         from smsSucceeds in Arb.Generate<bool>()
         select new CriticalNotificationSmsFallbackCase(
             eventType,
             pushSucceeds,
             includeSmsChannel,
             smsSucceeds))
        .ToArbitrary();
}
