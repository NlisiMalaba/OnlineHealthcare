using HealthPlatform.Application.Notifications;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Notifications;

public sealed class StoredNotificationPreferenceResolverTests
{
    [Fact]
    public async Task ResolveEnabledChannelsAsync_Returns_only_enabled_channels_from_store()
    {
        var preferenceService = new Mock<INotificationPreferenceService>();
        preferenceService
            .Setup(service => service.GetStoredPreferencesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new StoredNotificationChannelPreference(
                    NotificationEventTypes.AppointmentConfirmed,
                    "sms",
                    false)
            ]);

        var resolver = new StoredNotificationPreferenceResolver(preferenceService.Object);
        var channels = await resolver.ResolveEnabledChannelsAsync(
            Guid.CreateVersion7(),
            NotificationEventTypes.AppointmentConfirmed,
            NotificationCriticality.Standard,
            CancellationToken.None);

        Assert.Equal(
            [NotificationChannel.Push, NotificationChannel.Email],
            channels);
    }

    [Fact]
    public async Task ResolveEnabledChannelsAsync_Without_user_returns_all_channels()
    {
        var preferenceService = new Mock<INotificationPreferenceService>(MockBehavior.Strict);
        var resolver = new StoredNotificationPreferenceResolver(preferenceService.Object);

        var channels = await resolver.ResolveEnabledChannelsAsync(
            null,
            NotificationEventTypes.EmergencyAlert,
            NotificationCriticality.Critical,
            CancellationToken.None);

        Assert.Equal(NotificationPreferenceDefaults.AllChannels, channels);
    }

    [Fact]
    public async Task ResolveEnabledChannelsAsync_Filters_disabled_channels_for_requested_event_only()
    {
        var preferenceService = new Mock<INotificationPreferenceService>();
        preferenceService
            .Setup(service => service.GetStoredPreferencesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new StoredNotificationChannelPreference(
                    NotificationEventTypes.MedicationDoseReminder,
                    "push",
                    false),
                new StoredNotificationChannelPreference(
                    NotificationEventTypes.AppointmentConfirmed,
                    "sms",
                    false)
            ]);

        var resolver = new StoredNotificationPreferenceResolver(preferenceService.Object);
        var userId = Guid.CreateVersion7();

        var medicationChannels = await resolver.ResolveEnabledChannelsAsync(
            userId,
            NotificationEventTypes.MedicationDoseReminder,
            NotificationCriticality.Critical,
            CancellationToken.None);

        var appointmentChannels = await resolver.ResolveEnabledChannelsAsync(
            userId,
            NotificationEventTypes.AppointmentConfirmed,
            NotificationCriticality.Standard,
            CancellationToken.None);

        Assert.Equal([NotificationChannel.Email, NotificationChannel.Sms], medicationChannels);
        Assert.Equal([NotificationChannel.Push, NotificationChannel.Email], appointmentChannels);
    }
}
