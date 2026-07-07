using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Labs;
using HealthPlatform.Application.Labs.GetPatientLabResultDownload;
using HealthPlatform.Application.Storage;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Labs;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Labs;

public sealed class GetPatientLabResultDownloadQueryHandlerTests
{
    [Fact]
    public async Task Handle_returns_signed_download_url_for_patients_own_result()
    {
        var patientUserId = Guid.CreateVersion7();
        var patient = Patient.RegisterWithEmail(patientUserId, "Lab Patient", "lab-patient@example.com");
        var storageKey = $"patients/{patient.Id:N}/lab-results/cbc.pdf";
        var signedUrl = $"file:///{storageKey}";

        var labResult = LabResult.Create(
            Guid.CreateVersion7(),
            patient.Id,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "LABX",
            "REF-42",
            "CBC",
            storageKey,
            "application/pdf",
            "cbc.pdf",
            false);

        var patientRepository = new Mock<IPatientRepository>();
        patientRepository.Setup(x => x.GetByUserIdAsync(patientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var labResultRepository = new Mock<ILabResultRepository>();
        labResultRepository.Setup(x => x.GetByIdAsync(labResult.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(labResult);

        var storageService = new Mock<IStorageService>();
        storageService.Setup(x => x.GetSignedReadUrlAsync(storageKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(signedUrl);

        var auditService = new Mock<IHealthRecordAccessAuditService>();

        var handler = new GetPatientLabResultDownloadQueryHandler(
            new TestCurrentUserAccessor { UserId = patientUserId },
            patientRepository.Object,
            labResultRepository.Object,
            storageService.Object,
            auditService.Object);

        var result = await handler.Handle(
            new GetPatientLabResultDownloadQuery(labResult.Id),
            CancellationToken.None);

        Assert.Equal(labResult.Id, result.LabResultId);
        Assert.Equal("cbc.pdf", result.FileName);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal(signedUrl, result.DownloadUrl);

        auditService.Verify(
            x => x.LogPatientAccessAsync(
                patient.Id,
                labResult.HealthRecordId,
                HealthRecordAccessOperations.DownloadLabResult,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_throws_not_found_when_result_belongs_to_another_patient()
    {
        var patientUserId = Guid.CreateVersion7();
        var patient = Patient.RegisterWithEmail(patientUserId, "Lab Patient", "lab-patient@example.com");
        var otherPatientId = Guid.CreateVersion7();

        var labResult = LabResult.Create(
            Guid.CreateVersion7(),
            otherPatientId,
            Guid.CreateVersion7(),
            null,
            "LABX",
            "REF-99",
            "CBC",
            "patients/other/lab.pdf",
            "application/pdf",
            "cbc.pdf",
            false);

        var patientRepository = new Mock<IPatientRepository>();
        patientRepository.Setup(x => x.GetByUserIdAsync(patientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var labResultRepository = new Mock<ILabResultRepository>();
        labResultRepository.Setup(x => x.GetByIdAsync(labResult.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(labResult);

        var handler = new GetPatientLabResultDownloadQueryHandler(
            new TestCurrentUserAccessor { UserId = patientUserId },
            patientRepository.Object,
            labResultRepository.Object,
            Mock.Of<IStorageService>(),
            Mock.Of<IHealthRecordAccessAuditService>());

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new GetPatientLabResultDownloadQuery(labResult.Id), CancellationToken.None));

        Assert.Equal(LabOrderErrorCodes.LabResultNotFound, exception.Code);
    }
}
