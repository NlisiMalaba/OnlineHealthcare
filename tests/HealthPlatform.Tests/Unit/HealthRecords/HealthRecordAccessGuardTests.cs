using HealthPlatform.Application.Audit;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Domain.Audit;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Infrastructure.MongoDb;
using HealthPlatform.Infrastructure.Persistence.Repositories;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.HealthRecords;

public sealed class HealthRecordAccessGuardTests
{
    [Fact]
    public async Task EnsureDoctorCanReadAsync_writes_audit_log_and_throws_when_grant_missing()
    {
        var healthRecordId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var auditLogs = new List<AuditLog>();
        var auditRepository = new Mock<IAuditLogRepository>();
        auditRepository
            .Setup(repo => repo.AppendAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => auditLogs.Add(log))
            .Returns(Task.CompletedTask);

        var accessRepository = new Mock<IHealthRecordAccessRepository>();
        accessRepository
            .Setup(repo => repo.GetActiveGrantAsync(healthRecordId, doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((HealthRecordAccess?)null);

        var guard = new HealthRecordAccessGuard(
            accessRepository.Object,
            auditRepository.Object,
            new TestAuditContextAccessor(),
            new FakeTimeProvider(new DateTime(2026, 7, 3, 9, 0, 0, DateTimeKind.Utc)));

        var exception = await Assert.ThrowsAsync<AccessDeniedException>(() =>
            guard.EnsureDoctorCanReadAsync(healthRecordId, doctorId, CancellationToken.None));

        Assert.Equal("ACCESS_DENIED", exception.Code);
        Assert.Single(auditLogs);
        Assert.Equal(AuditActions.HealthRecordAccessDenied, auditLogs[0].Action);
        Assert.Equal(doctorId, auditLogs[0].ActorId);
        Assert.Equal(healthRecordId, auditLogs[0].ResourceId);
    }

    [Fact]
    public async Task EnsureDoctorCanReadAsync_succeeds_when_active_grant_exists()
    {
        var healthRecordId = Guid.CreateVersion7();
        var doctorId = Guid.CreateVersion7();
        var grant = HealthRecordAccess.Grant(
            healthRecordId,
            doctorId,
            HealthRecordAccessType.Full,
            sections: null,
            DateTime.UtcNow);

        var accessRepository = new Mock<IHealthRecordAccessRepository>();
        accessRepository
            .Setup(repo => repo.GetActiveGrantAsync(healthRecordId, doctorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grant);

        var guard = new HealthRecordAccessGuard(
            accessRepository.Object,
            Mock.Of<IAuditLogRepository>(),
            new TestAuditContextAccessor(),
            TimeProvider.System);

        await guard.EnsureDoctorCanReadAsync(healthRecordId, doctorId, CancellationToken.None);
    }
}
