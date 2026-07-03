using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.HealthRecords.RevokeHealthRecordAccess;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.HealthRecords;

public sealed class RevokeHealthRecordAccessCommandHandlerTests
{
    [Fact]
    public async Task Handle_revokes_active_grant_and_writes_audit_log()
    {
        var patientUserId = Guid.CreateVersion7();
        var patient = Patient.RegisterWithEmail(patientUserId, "Revoke Patient", "revoke@example.com");
        var doctorId = Guid.CreateVersion7();
        var healthRecord = HealthRecord.CreateForPatient(patient.Id);
        var revokedAt = new DateTime(2026, 7, 3, 11, 30, 0, DateTimeKind.Utc);
        var grant = HealthRecordAccess.Grant(
            healthRecord.Id,
            doctorId,
            HealthRecordAccessType.Full,
            sections: null,
            revokedAt.AddDays(-2));

        var currentUser = new TestCurrentUserAccessor { UserId = patientUserId };
        var patientRepository = new Mock<IPatientRepository>();
        patientRepository
            .Setup(repo => repo.GetByUserIdAsync(patientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var doctor = CreateDoctor(doctorId);
        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository
            .Setup(repo => repo.GetByIdAsync(doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        var healthRecordRepository = new Mock<IHealthRecordRepository>();
        healthRecordRepository
            .Setup(repo => repo.GetByPatientIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthRecord);

        var accessRepository = new Mock<IHealthRecordAccessRepository>();
        accessRepository
            .Setup(repo => repo.GetLatestGrantAsync(healthRecord.Id, doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grant);

        var auditService = new Mock<IHealthRecordAccessAuditService>();

        var handler = new RevokeHealthRecordAccessCommandHandler(
            currentUser,
            patientRepository.Object,
            doctorRepository.Object,
            healthRecordRepository.Object,
            accessRepository.Object,
            auditService.Object,
            new FakeTimeProvider(revokedAt));

        var result = await handler.Handle(new RevokeHealthRecordAccessCommand(doctorId), CancellationToken.None);

        Assert.False(result.IsActive);
        Assert.Equal(revokedAt, result.RevokedAtUtc);
        Assert.Equal(doctorId, result.DoctorId);

        accessRepository.Verify(
            repo => repo.UpdateAsync(grant, It.IsAny<CancellationToken>()),
            Times.Once);

        auditService.Verify(
            service => service.LogRevokeAsync(
                patient.Id,
                healthRecord.Id,
                doctorId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_throws_when_active_grant_missing()
    {
        var patientUserId = Guid.CreateVersion7();
        var patient = Patient.RegisterWithEmail(patientUserId, "Missing Grant Patient", "missing@example.com");
        var doctorId = Guid.CreateVersion7();
        var healthRecord = HealthRecord.CreateForPatient(patient.Id);

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
            .Setup(repo => repo.GetLatestGrantAsync(healthRecord.Id, doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HealthRecordAccess?)null);

        var handler = new RevokeHealthRecordAccessCommandHandler(
            currentUser,
            patientRepository.Object,
            doctorRepository.Object,
            healthRecordRepository.Object,
            accessRepository.Object,
            Mock.Of<IHealthRecordAccessAuditService>(),
            TimeProvider.System);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new RevokeHealthRecordAccessCommand(doctorId), CancellationToken.None));

        Assert.Equal(HealthRecordErrorCodes.HealthRecordAccessNotFound, exception.Code);
    }

    [Fact]
    public async Task Handle_throws_when_grant_already_revoked()
    {
        var patientUserId = Guid.CreateVersion7();
        var patient = Patient.RegisterWithEmail(patientUserId, "Revoked Grant Patient", "revoked@example.com");
        var doctorId = Guid.CreateVersion7();
        var healthRecord = HealthRecord.CreateForPatient(patient.Id);
        var grant = HealthRecordAccess.Grant(
            healthRecord.Id,
            doctorId,
            HealthRecordAccessType.ReadOnly,
            sections: null,
            DateTime.UtcNow.AddDays(-5));
        grant.Revoke(DateTime.UtcNow.AddDays(-1));

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
            .Setup(repo => repo.GetLatestGrantAsync(healthRecord.Id, doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grant);

        var handler = new RevokeHealthRecordAccessCommandHandler(
            currentUser,
            patientRepository.Object,
            doctorRepository.Object,
            healthRecordRepository.Object,
            accessRepository.Object,
            Mock.Of<IHealthRecordAccessAuditService>(),
            TimeProvider.System);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new RevokeHealthRecordAccessCommand(doctorId), CancellationToken.None));
    }

    private static Doctor CreateDoctor(Guid doctorId) =>
        Doctor.Register(
            doctorId,
            Guid.CreateVersion7(),
            "Dr. Revoke",
            "HPCZ-REVOKE",
            "General Practice",
            8,
            "Harare",
            null,
            30m,
            45m,
            null,
            "revoke-doctor@example.com",
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
}
