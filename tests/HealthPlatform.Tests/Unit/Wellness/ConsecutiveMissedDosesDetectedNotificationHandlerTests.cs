using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.Wellness.EventHandlers;
using HealthPlatform.Application.Wellness.Notifications;
using HealthPlatform.Domain.NextOfKin;
using HealthPlatform.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class ConsecutiveMissedDosesDetectedNotificationHandlerTests
{
    [Fact]
    public async Task Handle_notifies_all_next_of_kin_contacts()
    {
        var patientId = Guid.CreateVersion7();
        var contactOne = NextOfKinContact.Create(
            patientId,
            "Contact One",
            "Parent",
            "+15550001000",
            "one@example.com",
            false);
        var contactTwo = NextOfKinContact.Create(
            patientId,
            "Contact Two",
            "Sibling",
            "+15550002000",
            "two@example.com",
            true);

        var repository = new Mock<INextOfKinRepository>();
        repository
            .Setup(repo => repo.ListByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([contactOne, contactTwo]);

        var notifier = new CapturingConsecutiveMissedDosesNextOfKinNotifier();
        var handler = new ConsecutiveMissedDosesDetectedNotificationHandler(
            repository.Object,
            notifier,
            NullLogger<ConsecutiveMissedDosesDetectedNotificationHandler>.Instance);

        var triggeringEventId = Guid.CreateVersion7();
        await handler.Handle(
            new ConsecutiveMissedDosesDetectedNotification(
                patientId,
                triggeringEventId,
                DateTime.UtcNow,
                DateTime.UtcNow),
            CancellationToken.None);

        Assert.Single(notifier.Calls);
        Assert.Equal(2, notifier.Calls[0].ContactIds.Count);
        Assert.Contains(contactOne.Id, notifier.Calls[0].ContactIds);
        Assert.Contains(contactTwo.Id, notifier.Calls[0].ContactIds);
        Assert.Equal(triggeringEventId, notifier.Calls[0].TriggeringAdherenceEventId);
    }
}
