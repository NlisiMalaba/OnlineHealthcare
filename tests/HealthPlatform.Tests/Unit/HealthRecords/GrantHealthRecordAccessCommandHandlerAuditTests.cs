using HealthPlatform.Application.Audit;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.HealthRecords.GrantHealthRecordAccess;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.HealthRecords;

public sealed class GrantHealthRecordAccessCommandHandlerAuditTests
{
    [Fact]
    public async Task Handle_writes_grant_audit_log_for_new_access()
    {
        var patientUserId = Guid.CreateVersion7();
        var patient = Patient.RegisterWithEmail(patientUserId, "Grant Audit Patient", "grant-audit@example.com");
        var doctorId = Guid.CreateVersion7();
        var healthRecord = HealthRecord.CreateForPatient(patient.Id);
        var grantedAt = new DateTime(2026, 7, 3, 12, 0, 0, DateTimeKind.Utc);

        var currentUser = new TestCurrentUserAccessor { UserId = patientUserId };
        var patientRepository = new Mock<IPatientRepository>();
        patientRepository
            .Setup(repo => repo.GetByUserIdAsync(patientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository
            .Setup(repo => repo.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateDoctor(doctorId));

        var healthRecordRepository = new Mock<IHealthRecordRepository>();
        healthRecordRepository
            .Setup(repo => repo.GetByPatientIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthRecord);

        var accessRepository = new Mock<IHealthRecordAccessRepository>();
        accessRepository
            .Setup(repo => repo.GetActiveGrantAsync(healthRecord.Id, doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HealthRecordAccess?)null);
        accessRepository
            .Setup(repo => repo.GetLatestGrantAsync(healthRecord.Id, doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HealthRecordAccess?)null);

        var auditService = new Mock<IHealthRecordAccessAuditService>();

        var handler = new GrantHealthRecordAccessCommandHandler(
            currentUser,
            patientRepository.Object,
            doctorRepository.Object,
            healthRecordRepository.Object,
            accessRepository.Object,
            auditService.Object,
            new FakeTimeProvider(grantedAt));

        await handler.Handle(
            new GrantHealthRecordAccessCommand(doctorId, HealthRecordAccessType.Full, null),
            CancellationToken.None);

        auditService.Verify(
            service => service.LogGrantAsync(
                patient.Id,
                healthRecord.Id,
                doctorId,
                HealthRecordAccessType.Full,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Doctor CreateDoctor(Guid doctorId) =>
        Doctor.Register(
            doctorId,
            Guid.CreateVersion7(),
            "Dr. Grant",
            "HPCZ-GRANT",
            "General Practice",
            6,
            "Harare",
            null,
            25m,
            40m,
            null,
            "grant-doctor@example.com",
            "+263771234568",
            null,
            null,
            [
                DoctorAvailabilitySlot.Create(
                    doctorId,
                    DayOfWeek.Tuesday,
                    new TimeOnly(10, 0),
                    new TimeOnly(13, 0),
                    30,
                    DoctorAppointmentType.Both)
            ]);
}
