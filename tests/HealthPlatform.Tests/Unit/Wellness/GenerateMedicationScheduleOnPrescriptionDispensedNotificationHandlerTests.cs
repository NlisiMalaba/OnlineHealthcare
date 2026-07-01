using HealthPlatform.Application.Prescriptions.Notifications;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Application.Wellness.EventHandlers;
using HealthPlatform.Domain.Wellness;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class GenerateMedicationScheduleOnPrescriptionDispensedNotificationHandlerTests
{
  [Fact]
  public async Task Handle_creates_schedule_with_dose_times_for_dispensed_prescription()
  {
    var prescriptionId = Guid.CreateVersion7();
    var patientId = Guid.CreateVersion7();
    var dispensedAtUtc = new DateTime(2026, 6, 24, 7, 0, 0, DateTimeKind.Utc);
    var repository = new Mock<IMedicationScheduleRepository>();
    MedicationSchedule? createdSchedule = null;

    repository
        .Setup(r => r.GetByPrescriptionIdAsync(prescriptionId, It.IsAny<CancellationToken>()))
        .ReturnsAsync((MedicationSchedule?)null);
    repository
        .Setup(r => r.AddAsync(It.IsAny<MedicationSchedule>(), It.IsAny<CancellationToken>()))
        .Callback<MedicationSchedule, CancellationToken>((schedule, _) => createdSchedule = schedule)
        .Returns(Task.CompletedTask);

    var handler = new GenerateMedicationScheduleOnPrescriptionDispensedNotificationHandler(
        repository.Object,
        NullLogger<GenerateMedicationScheduleOnPrescriptionDispensedNotificationHandler>.Instance);

    await handler.Handle(
        new PrescriptionDispensedNotification(
            prescriptionId,
            patientId,
            "Amoxicillin",
            "500mg",
            "Twice daily",
            7,
            dispensedAtUtc,
            dispensedAtUtc),
        CancellationToken.None);

    Assert.NotNull(createdSchedule);
    Assert.Equal(prescriptionId, createdSchedule!.PrescriptionId);
    Assert.Equal(patientId, createdSchedule.PatientId);
    Assert.Equal("Amoxicillin", createdSchedule.MedicationName);
    Assert.Equal(MedicationScheduleStatus.Active, createdSchedule.Status);
    Assert.Equal(14, createdSchedule.DoseTimes.Count);
    repository.Verify(
        r => r.AddAsync(It.IsAny<MedicationSchedule>(), It.IsAny<CancellationToken>()),
        Times.Once);
  }

  [Fact]
  public async Task Handle_is_idempotent_when_schedule_already_exists()
  {
    var prescriptionId = Guid.CreateVersion7();
    var existingSchedule = MedicationSchedule.CreateActive(
        prescriptionId,
        Guid.CreateVersion7(),
        "Amoxicillin",
        [new DateTime(2026, 6, 24, 8, 0, 0, DateTimeKind.Utc)]);

    var repository = new Mock<IMedicationScheduleRepository>();
    repository
        .Setup(r => r.GetByPrescriptionIdAsync(prescriptionId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingSchedule);

    var handler = new GenerateMedicationScheduleOnPrescriptionDispensedNotificationHandler(
        repository.Object,
        NullLogger<GenerateMedicationScheduleOnPrescriptionDispensedNotificationHandler>.Instance);

    await handler.Handle(
        new PrescriptionDispensedNotification(
            prescriptionId,
            existingSchedule.PatientId,
            "Amoxicillin",
            "500mg",
            "Twice daily",
            7,
            DateTime.UtcNow,
            DateTime.UtcNow),
        CancellationToken.None);

    repository.Verify(
        r => r.AddAsync(It.IsAny<MedicationSchedule>(), It.IsAny<CancellationToken>()),
        Times.Never);
  }
}
