using HealthPlatform.Application.Notifications;
using Xunit;

namespace HealthPlatform.Tests.Unit.Notifications;

public sealed class NotificationPreferenceDefaultsTests
{
    [Fact]
    public void ResolveEnabledChannels_Treats_missing_preference_as_enabled()
    {
        var channels = NotificationPreferenceDefaults.ResolveEnabledChannels(
            NotificationEventTypes.AppointmentConfirmed,
            []);

        Assert.Equal(NotificationPreferenceDefaults.AllChannels, channels);
    }

    [Fact]
    public void ResolveEnabledChannels_Excludes_only_disabled_channels_for_matching_event()
    {
        var channels = NotificationPreferenceDefaults.ResolveEnabledChannels(
            NotificationEventTypes.MedicationDoseReminder,
            [
                new StoredNotificationChannelPreference(
                    NotificationEventTypes.MedicationDoseReminder,
                    "push",
                    false),
                new StoredNotificationChannelPreference(
                    NotificationEventTypes.MedicationDoseReminder,
                    "email",
                    false),
                new StoredNotificationChannelPreference(
                    NotificationEventTypes.AppointmentConfirmed,
                    "sms",
                    false)
            ]);

        Assert.Equal([NotificationChannel.Sms], channels);
    }
}
