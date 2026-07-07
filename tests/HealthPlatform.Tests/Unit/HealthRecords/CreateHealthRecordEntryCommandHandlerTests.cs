using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.HealthRecords.CreateHealthRecordEntry;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Infrastructure.MongoDb;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.HealthRecords;

public sealed class CreateHealthRecordEntryCommandHandlerTests
{
    [Fact]
    public async Task Handle_creates_consultation_note_entry()
    {
        var patientId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var healthRecord = HealthRecord.CreateForPatient(patientId);
        var doctorUserId = Guid.CreateVersion7();
        var entryRepository = new InMemoryHealthRecordEntryRepository();
        var currentUser = new TestCurrentUserAccessor { UserId = doctorUserId };
        var timeProvider = new FakeTimeProvider(new DateTime(2026, 7, 3, 8, 0, 0, DateTimeKind.Utc));

        var healthRecordRepository = new Mock<IHealthRecordRepository>();
        healthRecordRepository
            .Setup(repo => repo.GetByIdAsync(healthRecord.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthRecord);

        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository
            .Setup(repo => repo.GetByUserIdWithSlotsAsync(doctorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateVerifiedDoctor(doctorId, doctorUserId));

        var handler = new CreateHealthRecordEntryCommandHandler(
            currentUser,
            doctorRepository.Object,
            healthRecordRepository.Object,
            entryRepository,
            timeProvider);

        var result = await handler.Handle(
            new CreateHealthRecordEntryCommand(
                healthRecord.Id,
                HealthRecordEntryType.ConsultationNote,
                new HealthRecordEntryContentPayload(
                    ConsultationNote: new ConsultationNoteContent("Patient reports mild headache.", null)),
                IsVisibleToPatient: true),
            CancellationToken.None);

        Assert.Equal(HealthRecordEntryType.ConsultationNote, result.EntryType);
        Assert.Equal(doctorId, result.AuthoredBy);
        Assert.Equal("Patient reports mild headache.", result.Content.ConsultationNote!.Notes);
        Assert.Single(entryRepository.Entries);
    }

    private static Doctor CreateVerifiedDoctor(Guid doctorId, Guid userId)
    {
        var doctor = Doctor.Register(
            doctorId,
            userId,
            "Dr. Verified",
            "HPCZ-123",
            "General Practice",
            5,
            "Harare",
            null,
            20m,
            35m,
            null,
            "verified@example.com",
            "+263771234567",
            null,
            null,
            [
                DoctorAvailabilitySlot.Create(
                    doctorId,
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(12, 0),
                    30,
                    DoctorAppointmentType.Both)
            ]);

        doctor.VerifyLicense();
        return doctor;
    }
}
