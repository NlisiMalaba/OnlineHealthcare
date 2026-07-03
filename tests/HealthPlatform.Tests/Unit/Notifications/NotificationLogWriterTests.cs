using HealthPlatform.Application.Notifications;
using HealthPlatform.Domain.Notifications;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Notifications;

public sealed class NotificationLogWriterTests
{
    [Fact]
    public async Task RecordDispatchAsync_Persists_entry_per_channel_with_status_and_timestamp()
    {
        var recipientId = Guid.CreateVersion7();
        var sentAt = new DateTime(2026, 7, 3, 10, 0, 0, DateTimeKind.Utc);
        var timeProvider = new FakeTimeProvider(sentAt);
        var repository = new Mock<INotificationLogRepository>();
        IReadOnlyList<NotificationLog>? captured = null;
        repository
            .Setup(repo => repo.AddRangeAsync(It.IsAny<IReadOnlyList<NotificationLog>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<NotificationLog>, CancellationToken>((entries, _) => captured = entries)
            .Returns(Task.CompletedTask);

        var writer = new NotificationLogWriter(timeProvider, repository.Object);
        var request = new NotificationDispatchRequest(
            recipientId,
            NotificationRecipientType.Patient,
            NotificationEventTypes.AppointmentConfirmed,
            NotificationCriticality.Standard,
            new NotificationContent("Title", "Body"),
            Metadata: new Dictionary<string, string>
            {
                ["appointment_id"] = Guid.CreateVersion7().ToString()
            },
            Channels: [NotificationChannel.Push, NotificationChannel.Email]);

        await writer.RecordDispatchAsync(
            request,
            new ResolvedNotificationRecipient(
                recipientId,
                NotificationRecipientType.Patient,
                "patient@example.com",
                "+263771111111",
                ["token-1"]),
            [
                new ChannelDeliveryResult(NotificationChannel.Push, true),
                new ChannelDeliveryResult(NotificationChannel.Email, false, "DELIVERY_FAILED")
            ],
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(2, captured!.Count);

        var pushEntry = captured.Single(entry => entry.Channel == "push");
        Assert.Equal(recipientId, pushEntry.RecipientId);
        Assert.Equal("patient", pushEntry.RecipientType);
        Assert.Equal(NotificationEventTypes.AppointmentConfirmed, pushEntry.EventType);
        Assert.Equal(NotificationDeliveryStatus.Sent, pushEntry.Status);
        Assert.Equal(sentAt, pushEntry.SentAtUtc);
        Assert.Null(pushEntry.DeliveredAtUtc);
        Assert.Equal(1, pushEntry.Attempts);

        var emailEntry = captured.Single(entry => entry.Channel == "email");
        Assert.Equal(NotificationDeliveryStatus.Failed, emailEntry.Status);
        Assert.Equal("DELIVERY_FAILED", emailEntry.FailureReason);

        repository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Resolve_Uses_contact_id_when_user_id_missing()
    {
        var contactId = Guid.CreateVersion7();
        var resolved = NotificationLogRecipientIdResolver.Resolve(
            new NotificationDispatchRequest(
                null,
                NotificationRecipientType.NextOfKin,
                NotificationEventTypes.EmergencyAlert,
                NotificationCriticality.Critical,
                new NotificationContent("Alert", "Body"),
                Metadata: new Dictionary<string, string>
                {
                    ["contact_id"] = contactId.ToString()
                },
                Channels: [NotificationChannel.Sms]));

        Assert.Equal(contactId, resolved);
    }

    private sealed class FakeTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }
}
