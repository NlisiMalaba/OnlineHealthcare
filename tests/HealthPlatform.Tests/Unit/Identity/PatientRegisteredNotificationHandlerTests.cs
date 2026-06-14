using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Identity.EventHandlers;
using HealthPlatform.Application.Identity.Notifications;
using HealthPlatform.Domain.HealthRecords;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Identity;

public sealed class PatientRegisteredNotificationHandlerTests
{
    [Fact]
    public async Task Handle_WhenHealthRecordMissing_CreatesHealthRecord()
    {
        var patientId = Guid.CreateVersion7();
        var repository = new Mock<IHealthRecordRepository>();
        repository.Setup(r => r.ExistsForPatientAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        HealthRecord? created = null;
        repository.Setup(r => r.AddAsync(It.IsAny<HealthRecord>(), It.IsAny<CancellationToken>()))
            .Callback<HealthRecord, CancellationToken>((record, _) => created = record)
            .Returns(Task.CompletedTask);
        repository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new PatientRegisteredNotificationHandler(
            repository.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<PatientRegisteredNotificationHandler>>());

        await handler.Handle(
            new PatientRegisteredNotification(patientId, DateTime.UtcNow),
            CancellationToken.None);

        Assert.NotNull(created);
        Assert.Equal(patientId, created!.PatientId);
    }

    [Fact]
    public async Task Handle_WhenHealthRecordExists_IsIdempotent()
    {
        var patientId = Guid.CreateVersion7();
        var repository = new Mock<IHealthRecordRepository>();
        repository.Setup(r => r.ExistsForPatientAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new PatientRegisteredNotificationHandler(
            repository.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<PatientRegisteredNotificationHandler>>());

        await handler.Handle(
            new PatientRegisteredNotification(patientId, DateTime.UtcNow),
            CancellationToken.None);

        repository.Verify(
            r => r.AddAsync(It.IsAny<HealthRecord>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
