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
}
