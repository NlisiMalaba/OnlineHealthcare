using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Prescriptions;
using HealthPlatform.Application.Prescriptions.EventHandlers;
using HealthPlatform.Application.Prescriptions.Notifications;
using HealthPlatform.Domain.Identity;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Prescriptions;

public sealed class PrescriptionIssuedNotificationHandlerTests
{
    [Fact]
    public async Task Handle_notifies_patient_when_prescription_is_issued()
    {
        var patient = Patient.RegisterWithEmail(
            Guid.CreateVersion7(),
            "Patient One",
            "patient1@example.com");

        var patientRepository = new Mock<IPatientRepository>();
        patientRepository
            .Setup(repo => repo.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var notifier = new Mock<IPrescriptionIssuedNotifier>();
        var handler = new PrescriptionIssuedNotificationHandler(
            patientRepository.Object,
            notifier.Object);

        var issuedAtUtc = DateTime.UtcNow;
        var notification = new PrescriptionIssuedNotification(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            patient.Id,
            Guid.CreateVersion7(),
            issuedAtUtc,
            issuedAtUtc.AddDays(30),
            DateTime.UtcNow);

        await handler.Handle(notification, CancellationToken.None);

        notifier.Verify(x => x.NotifyPrescriptionIssuedAsync(
            patient.UserId,
            notification.PrescriptionId,
            notification.IssuedAtUtc,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_throws_when_patient_is_missing()
    {
        var patientRepository = new Mock<IPatientRepository>();
        patientRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        var handler = new PrescriptionIssuedNotificationHandler(
            patientRepository.Object,
            Mock.Of<IPrescriptionIssuedNotifier>());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(
            new PrescriptionIssuedNotification(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(30),
                DateTime.UtcNow),
            CancellationToken.None));
    }
}
