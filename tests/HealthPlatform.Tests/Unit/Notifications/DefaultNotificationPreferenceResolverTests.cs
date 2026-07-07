using HealthPlatform.Application.Notifications;
using Xunit;

namespace HealthPlatform.Tests.Unit.Notifications;

public sealed class DefaultNotificationPreferenceResolverTests
{
    [Fact]
    public async Task ResolveEnabledChannelsAsync_ReturnsPushEmailAndSmsByDefault()
    {
        var resolver = new DefaultNotificationPreferenceResolver();

        var channels = await resolver.ResolveEnabledChannelsAsync(
            Guid.CreateVersion7(),
            NotificationEventTypes.AppointmentConfirmed,
            NotificationCriticality.Standard,
            CancellationToken.None);

        Assert.Equal(
            [NotificationChannel.Push, NotificationChannel.Email, NotificationChannel.Sms],
            channels);
    }
}
