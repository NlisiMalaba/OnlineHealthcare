using HealthPlatform.Application.Audit;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.HealthRecords.ExportPatientHealthRecordPdf;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Storage;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.HealthRecords;

public sealed class ExportPatientHealthRecordPdfQueryHandlerTests
{
    [Fact]
    public async Task Handle_uploads_pdf_and_returns_signed_url_with_audit_log()
    {
        var patientUserId = Guid.CreateVersion7();
        var patient = Patient.RegisterWithEmail(patientUserId, "PDF Patient", "pdf-patient@example.com");
        var healthRecord = HealthRecord.CreateForPatient(patient.Id);
        var generatedAt = new DateTime(2026, 7, 3, 11, 0, 0, DateTimeKind.Utc);
        var storageKey = $"patients/{patient.Id:N}/health-records/{healthRecord.Id:N}/exports/test.pdf";
        var signedUrl = $"file:///{storageKey}";

        var currentUser = new TestCurrentUserAccessor { UserId = patientUserId };
        var patientRepository = new Mock<IPatientRepository>();
        patientRepository
            .Setup(repo => repo.GetByUserIdAsync(patientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var healthRecordRepository = new Mock<IHealthRecordRepository>();
        healthRecordRepository
            .Setup(repo => repo.GetByPatientIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthRecord);

        var entries = new List<HealthRecordEntryDto>
        {
            new(
                "entry-1",
                healthRecord.Id,
                HealthRecordEntryType.Allergy,
                new HealthRecordEntryContentPayload(
                    Allergy: new AllergyContent("Penicillin", "severe", "Rash")),
                Guid.CreateVersion7(),
                generatedAt.AddDays(-1),
                null,
                true)
        };

        var entryRepository = new Mock<IHealthRecordEntryRepository>();
        entryRepository
            .Setup(repo => repo.ListByHealthRecordIdAsync(healthRecord.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var pdfGenerator = new Mock<IHealthRecordPdfGenerator>();
        pdfGenerator
            .Setup(generator => generator.Generate(It.IsAny<PatientHealthRecordExportModel>()))
            .Returns([0x25, (byte)'P', (byte)'D', (byte)'F', 0x2D]);

        var storageService = new Mock<IStorageService>();
        storageService
            .Setup(service => service.UploadHealthRecordExportAsync(
                patient.Id,
                healthRecord.Id,
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StorageUploadResult(storageKey, "application/pdf"));
        storageService
            .Setup(service => service.GetSignedReadUrlAsync(storageKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(signedUrl);

        var auditService = new Mock<IHealthRecordAccessAuditService>();

        var handler = new ExportPatientHealthRecordPdfQueryHandler(
            currentUser,
            patientRepository.Object,
            healthRecordRepository.Object,
            entryRepository.Object,
            pdfGenerator.Object,
            auditService.Object,
            storageService.Object,
            new FakeTimeProvider(generatedAt));

        var result = await handler.Handle(new ExportPatientHealthRecordPdfQuery(), CancellationToken.None);

        Assert.Equal(healthRecord.Id, result.HealthRecordId);
        Assert.Equal(signedUrl, result.DownloadUrl);
        Assert.Equal(generatedAt, result.GeneratedAtUtc);

        pdfGenerator.Verify(
            generator => generator.Generate(
                It.Is<PatientHealthRecordExportModel>(model =>
                    model.HealthRecordId == healthRecord.Id
                    && model.PatientId == patient.Id
                    && model.Entries.Count == 1)),
            Times.Once);

        storageService.Verify(
            service => service.UploadHealthRecordExportAsync(
                patient.Id,
                healthRecord.Id,
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        auditService.Verify(
            service => service.LogPatientAccessAsync(
                patient.Id,
                healthRecord.Id,
                HealthRecordAccessOperations.ExportPdf,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
