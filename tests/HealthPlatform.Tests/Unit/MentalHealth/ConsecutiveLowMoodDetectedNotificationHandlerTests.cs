using HealthPlatform.Application.Identity;
using HealthPlatform.Application.MentalHealth.MoodLogs.EventHandlers;
using HealthPlatform.Application.MentalHealth.MoodLogs.Notifications;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.MentalHealth;

public sealed class ConsecutiveLowMoodDetectedNotificationHandlerTests
{
    [Fact]
    public async Task Handle_notifies_patient_user()
    {
        var patientId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var patient = Patient.RegisterWithEmail(userId, "Low Mood Patient", "low-mood@example.com");

        var repository = new Mock<IPatientRepository>();
        repository
            .Setup(repo => repo.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var notifier = new CapturingConsecutiveLowMoodPromptNotifier();
        var handler = new ConsecutiveLowMoodDetectedNotificationHandler(repository.Object, notifier);

        const string moodLogId = "mood-log-123";
        await handler.Handle(
            new ConsecutiveLowMoodDetectedNotification(
                patientId,
                moodLogId,
                DateTime.UtcNow,
                DateTime.UtcNow),
            CancellationToken.None);

        Assert.Single(notifier.Calls);
        Assert.Equal(userId, notifier.Calls[0].PatientUserId);
        Assert.Equal(patientId, notifier.Calls[0].PatientId);
        Assert.Equal(moodLogId, notifier.Calls[0].TriggeringMoodLogId);
    }
}
