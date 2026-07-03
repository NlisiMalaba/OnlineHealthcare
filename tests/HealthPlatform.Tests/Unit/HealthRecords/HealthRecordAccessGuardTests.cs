using HealthPlatform.Application.Audit;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Domain.Audit;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.HealthRecords;

public sealed class HealthRecordAccessGuardTests
{
    [Fact]
    public async Task EnsureDoctorCanReadAsync_logs_denied_attempt_and_throws_when_grant_missing()
    {
        var healthRecordId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var auditService = new Mock<IHealthRecordAccessAuditService>();

        var accessRepository = new Mock<IHealthRecordAccessRepository>();
        accessRepository
            .Setup(repo => repo.GetActiveGrantAsync(healthRecordId, doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HealthRecordAccess?)null);

        var guard = new HealthRecordAccessGuard(accessRepository.Object, auditService.Object);

        await Assert.ThrowsAsync<Application.Exceptions.AccessDeniedException>(() =>
            guard.EnsureDoctorCanReadAsync(
                healthRecordId,
                doctorId,
                HealthRecordAccessOperations.ListEntries,
                CancellationToken.None));

        auditService.Verify(
            service => service.LogDoctorAccessAttemptAsync(
                doctorId,
                healthRecordId,
                HealthRecordAccessOperations.ListEntries,
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EnsureDoctorCanReadAsync_logs_allowed_attempt_when_grant_exists()
    {
        var healthRecordId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var grant = HealthRecordAccess.Grant(
            healthRecordId,
            doctorId,
            HealthRecordAccessType.Full,
            sections: null,
            DateTime.UtcNow);

        var auditService = new Mock<IHealthRecordAccessAuditService>();
        var accessRepository = new Mock<IHealthRecordAccessRepository>();
        accessRepository
            .Setup(repo => repo.GetActiveGrantAsync(healthRecordId, doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grant);

        var guard = new HealthRecordAccessGuard(accessRepository.Object, auditService.Object);

        await guard.EnsureDoctorCanReadAsync(
            healthRecordId,
            doctorId,
            HealthRecordAccessOperations.GetEntry,
            CancellationToken.None);

        auditService.Verify(
            service => service.LogDoctorAccessAttemptAsync(
                doctorId,
                healthRecordId,
                HealthRecordAccessOperations.GetEntry,
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
