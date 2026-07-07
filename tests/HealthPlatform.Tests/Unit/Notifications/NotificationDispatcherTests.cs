using HealthPlatform.Application.Notifications;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Notifications;

public sealed class NotificationDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_UsesEnabledPreferenceChannels()
    {
        var preferenceResolver = new Mock<INotificationPreferenceResolver>();
        preferenceResolver
            .Setup(resolver => resolver.ResolveEnabledChannelsAsync(
                It.IsAny<Guid?>(),
                NotificationEventTypes.AppointmentConfirmed,
                NotificationCriticality.Standard,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([NotificationChannel.Email]);

        var recipientResolver = new Mock<INotificationRecipientResolver>();
        recipientResolver
            .Setup(resolver => resolver.ResolveAsync(
                It.IsAny<Guid>(),
                NotificationRecipientType.Patient,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResolvedNotificationRecipient(
                Guid.CreateVersion7(),
                NotificationRecipientType.Patient,
                "patient@example.com",
                "+263771111111",
                []));

        var emailGateway = new Mock<IEmailNotificationGateway>();
        emailGateway.SetupGet(gateway => gateway.Provider).Returns(NotificationChannelProviders.Logging);
        emailGateway.SetupGet(gateway => gateway.IsConfigured).Returns(true);
        emailGateway
            .Setup(gateway => gateway.TrySendAsync(It.IsAny<EmailNotificationDeliveryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var gatewayResolver = new Mock<INotificationChannelGatewayResolver>();
        gatewayResolver.Setup(resolver => resolver.ResolveEmail()).Returns(emailGateway.Object);

        var dispatcher = CreateDispatcher(
            preferenceResolver.Object,
            recipientResolver.Object,
            gatewayResolver.Object);

        var result = await dispatcher.DispatchAsync(
            new NotificationDispatchRequest(
                Guid.CreateVersion7(),
                NotificationRecipientType.Patient,
                NotificationEventTypes.AppointmentConfirmed,
                NotificationCriticality.Standard,
                new NotificationContent("Title", "Body")),
            CancellationToken.None);

        Assert.Single(result.ChannelResults);
        Assert.True(result.ChannelResults[0].Succeeded);
        emailGateway.Verify(
            gateway => gateway.TrySendAsync(It.IsAny<EmailNotificationDeliveryRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_WhenCriticalPushFails_SchedulesSmsFallback()
    {
        var pushGateway = new Mock<IPushNotificationGateway>();
        pushGateway.SetupGet(gateway => gateway.Provider).Returns(NotificationChannelProviders.Logging);
        pushGateway.SetupGet(gateway => gateway.IsConfigured).Returns(true);
        pushGateway
            .Setup(gateway => gateway.TrySendAsync(It.IsAny<PushNotificationDeliveryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var smsGateway = new Mock<ISmsNotificationGateway>();
        smsGateway.SetupGet(gateway => gateway.Provider).Returns(NotificationChannelProviders.Logging);
        smsGateway.SetupGet(gateway => gateway.IsConfigured).Returns(true);

        var gatewayResolver = new Mock<INotificationChannelGatewayResolver>();
        gatewayResolver.Setup(resolver => resolver.ResolvePush()).Returns(pushGateway.Object);
        gatewayResolver.Setup(resolver => resolver.ResolveSms()).Returns(smsGateway.Object);

        var smsFallbackService = new Mock<ICriticalNotificationSmsFallbackService>();
        smsFallbackService
            .Setup(service => service.ScheduleAsync(
                It.IsAny<NotificationDispatchRequest>(),
                It.IsAny<ResolvedNotificationRecipient>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var dispatcher = CreateDispatcher(
            new DefaultNotificationPreferenceResolver(),
            new FixedRecipientResolver(),
            gatewayResolver.Object,
            smsFallbackService: smsFallbackService.Object);

        var result = await dispatcher.DispatchAsync(
            new NotificationDispatchRequest(
                Guid.CreateVersion7(),
                NotificationRecipientType.Patient,
                NotificationEventTypes.MedicationDoseReminder,
                NotificationCriticality.Critical,
                new NotificationContent("Medication reminder", "Take your dose."),
                Channels: [NotificationChannel.Push]),
            CancellationToken.None);

        Assert.Single(result.ChannelResults);
        Assert.False(result.ChannelResults[0].Succeeded);
        smsFallbackService.Verify(
            service => service.ScheduleAsync(
                It.IsAny<NotificationDispatchRequest>(),
                It.IsAny<ResolvedNotificationRecipient>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        smsGateway.Verify(
            gateway => gateway.TrySendAsync(It.IsAny<SmsNotificationDeliveryRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DispatchAsync_WithContactOverride_DoesNotResolveUser()
    {
        var recipientResolver = new Mock<INotificationRecipientResolver>(MockBehavior.Strict);

        var smsGateway = new Mock<ISmsNotificationGateway>();
        smsGateway.SetupGet(gateway => gateway.Provider).Returns(NotificationChannelProviders.Logging);
        smsGateway.SetupGet(gateway => gateway.IsConfigured).Returns(true);
        smsGateway
            .Setup(gateway => gateway.TrySendAsync(It.IsAny<SmsNotificationDeliveryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var gatewayResolver = new Mock<INotificationChannelGatewayResolver>();
        gatewayResolver.Setup(resolver => resolver.ResolveSms()).Returns(smsGateway.Object);

        var dispatcher = CreateDispatcher(
            new DefaultNotificationPreferenceResolver(),
            recipientResolver.Object,
            gatewayResolver.Object);

        await dispatcher.DispatchAsync(
            new NotificationDispatchRequest(
                null,
                NotificationRecipientType.NextOfKin,
                NotificationEventTypes.EmergencyAlert,
                NotificationCriticality.Critical,
                new NotificationContent("Emergency alert", "Alert body"),
                new NotificationContactOverride("kin@example.com", "+263772222222"),
                Metadata: new Dictionary<string, string>
                {
                    ["contact_id"] = Guid.CreateVersion7().ToString()
                },
                Channels: [NotificationChannel.Sms]),
            CancellationToken.None);

        recipientResolver.Verify(
            resolver => resolver.ResolveAsync(It.IsAny<Guid>(), It.IsAny<NotificationRecipientType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DispatchAsync_WithStoredPreferences_DoesNotAttemptDisabledChannels()
    {
        var userId = Guid.CreateVersion7();
        var preferenceService = new Mock<INotificationPreferenceService>();
        preferenceService
            .Setup(service => service.GetStoredPreferencesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new StoredNotificationChannelPreference(
                    NotificationEventTypes.AppointmentConfirmed,
                    "push",
                    false),
                new StoredNotificationChannelPreference(
                    NotificationEventTypes.AppointmentConfirmed,
                    "sms",
                    false)
            ]);

        var preferenceResolver = new StoredNotificationPreferenceResolver(preferenceService.Object);

        var pushGateway = new Mock<IPushNotificationGateway>(MockBehavior.Strict);
        var smsGateway = new Mock<ISmsNotificationGateway>(MockBehavior.Strict);
        var emailGateway = new Mock<IEmailNotificationGateway>();
        emailGateway.SetupGet(gateway => gateway.Provider).Returns(NotificationChannelProviders.Logging);
        emailGateway.SetupGet(gateway => gateway.IsConfigured).Returns(true);
        emailGateway
            .Setup(gateway => gateway.TrySendAsync(It.IsAny<EmailNotificationDeliveryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var gatewayResolver = new Mock<INotificationChannelGatewayResolver>();
        gatewayResolver.Setup(resolver => resolver.ResolveEmail()).Returns(emailGateway.Object);

        var dispatcher = CreateDispatcher(
            preferenceResolver,
            new FixedRecipientResolver(),
            gatewayResolver.Object);

        var result = await dispatcher.DispatchAsync(
            new NotificationDispatchRequest(
                userId,
                NotificationRecipientType.Patient,
                NotificationEventTypes.AppointmentConfirmed,
                NotificationCriticality.Standard,
                new NotificationContent("Title", "Body")),
            CancellationToken.None);

        Assert.Single(result.ChannelResults);
        Assert.Equal(NotificationChannel.Email, result.ChannelResults[0].Channel);
        emailGateway.Verify(
            gateway => gateway.TrySendAsync(It.IsAny<EmailNotificationDeliveryRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
        preferenceService.Verify(
            service => service.GetStoredPreferencesAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_WithExplicitChannels_IgnoresPreferenceResolver()
    {
        var preferenceResolver = new Mock<INotificationPreferenceResolver>(MockBehavior.Strict);

        var smsGateway = new Mock<ISmsNotificationGateway>();
        smsGateway.SetupGet(gateway => gateway.Provider).Returns(NotificationChannelProviders.Logging);
        smsGateway.SetupGet(gateway => gateway.IsConfigured).Returns(true);
        smsGateway
            .Setup(gateway => gateway.TrySendAsync(It.IsAny<SmsNotificationDeliveryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var gatewayResolver = new Mock<INotificationChannelGatewayResolver>();
        gatewayResolver.Setup(resolver => resolver.ResolveSms()).Returns(smsGateway.Object);

        var dispatcher = CreateDispatcher(
            preferenceResolver.Object,
            new FixedRecipientResolver(),
            gatewayResolver.Object);

        await dispatcher.DispatchAsync(
            new NotificationDispatchRequest(
                Guid.CreateVersion7(),
                NotificationRecipientType.Patient,
                NotificationEventTypes.AppointmentConfirmed,
                NotificationCriticality.Standard,
                new NotificationContent("Title", "Body"),
                Channels: [NotificationChannel.Sms]),
            CancellationToken.None);

        smsGateway.Verify(
            gateway => gateway.TrySendAsync(It.IsAny<SmsNotificationDeliveryRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static NotificationDispatcher CreateDispatcher(
        INotificationPreferenceResolver preferenceResolver,
        INotificationRecipientResolver recipientResolver,
        INotificationChannelGatewayResolver gatewayResolver,
        ICriticalNotificationSmsFallbackService? smsFallbackService = null)
    {
        var logWriter = new Mock<INotificationLogWriter>();
        logWriter
            .Setup(writer => writer.RecordDispatchAsync(
                It.IsAny<NotificationDispatchRequest>(),
                It.IsAny<ResolvedNotificationRecipient>(),
                It.IsAny<IReadOnlyList<ChannelDeliveryResult>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        smsFallbackService ??= new Mock<ICriticalNotificationSmsFallbackService>().Object;

        return new NotificationDispatcher(
            preferenceResolver,
            recipientResolver,
            gatewayResolver,
            logWriter.Object,
            smsFallbackService,
            NullLogger<NotificationDispatcher>.Instance);
    }

    private sealed class FixedRecipientResolver : INotificationRecipientResolver
    {
        public Task<ResolvedNotificationRecipient> ResolveAsync(
            Guid userId,
            NotificationRecipientType recipientType,
            CancellationToken ct) =>
            Task.FromResult(new ResolvedNotificationRecipient(
                userId,
                recipientType,
                "patient@example.com",
                "+263771111111",
                []));
    }
}
