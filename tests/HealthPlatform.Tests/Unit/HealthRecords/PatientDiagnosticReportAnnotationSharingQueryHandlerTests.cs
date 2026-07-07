using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.HealthRecords.GetPatientHealthRecordEntry;
using HealthPlatform.Application.HealthRecords.ListPatientHealthRecordEntries;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.HealthRecords;

public sealed class PatientDiagnosticReportAnnotationSharingQueryHandlerTests
{
    [Fact]
    public async Task List_entries_includes_patient_visible_diagnostic_report_annotations()
    {
        var patientUserId = Guid.CreateVersion7();
        var patient = Patient.RegisterWithEmail(patientUserId, "Annotation Patient", "annotation@example.com");
        var healthRecord = HealthRecord.CreateForPatient(patient.Id);
        var annotatedAt = new DateTime(2026, 7, 7, 9, 0, 0, DateTimeKind.Utc);

        var annotationEntry = new HealthRecordEntryDto(
            "entry-annotation",
            healthRecord.Id,
            HealthRecordEntryType.DiagnosticReportAnnotation,
            new HealthRecordEntryContentPayload(
                DiagnosticReportAnnotation: new DiagnosticReportAnnotationContent(
                    "LabResult",
                    Guid.CreateVersion7(),
                    "Follow up in two weeks.",
                    annotatedAt)),
            Guid.CreateVersion7(),
            annotatedAt,
            null,
            true);

        var patientRepository = new Mock<IPatientRepository>();
        patientRepository.Setup(x => x.GetByUserIdAsync(patientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var healthRecordRepository = new Mock<IHealthRecordRepository>();
        healthRecordRepository.Setup(x => x.GetByPatientIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthRecord);

        var entryRepository = new Mock<IHealthRecordEntryRepository>();
        entryRepository.Setup(x => x.ListByHealthRecordIdAsync(healthRecord.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync([annotationEntry]);

        var auditService = new Mock<IHealthRecordAccessAuditService>();

        var handler = new ListPatientHealthRecordEntriesQueryHandler(
            new TestCurrentUserAccessor { UserId = patientUserId },
            patientRepository.Object,
            healthRecordRepository.Object,
            entryRepository.Object,
            auditService.Object);

        var entries = await handler.Handle(new ListPatientHealthRecordEntriesQuery(), CancellationToken.None);

        Assert.Single(entries);
        Assert.Equal(HealthRecordEntryType.DiagnosticReportAnnotation, entries[0].EntryType);
        Assert.True(entries[0].IsVisibleToPatient);
        Assert.Equal("Follow up in two weeks.", entries[0].Content.DiagnosticReportAnnotation!.Note);

        entryRepository.Verify(
            x => x.ListByHealthRecordIdAsync(healthRecord.Id, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Get_entry_returns_patient_visible_annotation()
    {
        var patientUserId = Guid.CreateVersion7();
        var patient = Patient.RegisterWithEmail(patientUserId, "Annotation Patient", "annotation@example.com");
        var healthRecord = HealthRecord.CreateForPatient(patient.Id);
        var annotatedAt = new DateTime(2026, 7, 7, 9, 30, 0, DateTimeKind.Utc);

        var annotationEntry = new HealthRecordEntryDto(
            "entry-annotation",
            healthRecord.Id,
            HealthRecordEntryType.DiagnosticReportAnnotation,
            new HealthRecordEntryContentPayload(
                DiagnosticReportAnnotation: new DiagnosticReportAnnotationContent(
                    "RadiologyReport",
                    Guid.CreateVersion7(),
                    "No further imaging required.",
                    annotatedAt)),
            Guid.CreateVersion7(),
            annotatedAt,
            null,
            true);

        var patientRepository = new Mock<IPatientRepository>();
        patientRepository.Setup(x => x.GetByUserIdAsync(patientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var healthRecordRepository = new Mock<IHealthRecordRepository>();
        healthRecordRepository.Setup(x => x.GetByPatientIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthRecord);

        var entryRepository = new Mock<IHealthRecordEntryRepository>();
        entryRepository.Setup(x => x.GetByIdAsync(annotationEntry.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(annotationEntry);

        var auditService = new Mock<IHealthRecordAccessAuditService>();

        var handler = new GetPatientHealthRecordEntryQueryHandler(
            new TestCurrentUserAccessor { UserId = patientUserId },
            patientRepository.Object,
            healthRecordRepository.Object,
            entryRepository.Object,
            auditService.Object);

        var entry = await handler.Handle(
            new GetPatientHealthRecordEntryQuery(annotationEntry.Id),
            CancellationToken.None);

        Assert.Equal(annotationEntry.Id, entry.Id);
        Assert.Equal("No further imaging required.", entry.Content.DiagnosticReportAnnotation!.Note);
    }

    [Fact]
    public async Task Get_entry_denies_patient_access_to_non_visible_annotation()
    {
        var patientUserId = Guid.CreateVersion7();
        var patient = Patient.RegisterWithEmail(patientUserId, "Annotation Patient", "annotation@example.com");
        var healthRecord = HealthRecord.CreateForPatient(patient.Id);

        var hiddenEntry = new HealthRecordEntryDto(
            "entry-hidden",
            healthRecord.Id,
            HealthRecordEntryType.DiagnosticReportAnnotation,
            new HealthRecordEntryContentPayload(
                DiagnosticReportAnnotation: new DiagnosticReportAnnotationContent(
                    "LabResult",
                    Guid.CreateVersion7(),
                    "Internal note",
                    DateTime.UtcNow)),
            Guid.CreateVersion7(),
            DateTime.UtcNow,
            null,
            false);

        var patientRepository = new Mock<IPatientRepository>();
        patientRepository.Setup(x => x.GetByUserIdAsync(patientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var healthRecordRepository = new Mock<IHealthRecordRepository>();
        healthRecordRepository.Setup(x => x.GetByPatientIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthRecord);

        var entryRepository = new Mock<IHealthRecordEntryRepository>();
        entryRepository.Setup(x => x.GetByIdAsync(hiddenEntry.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hiddenEntry);

        var auditService = new Mock<IHealthRecordAccessAuditService>();

        var handler = new GetPatientHealthRecordEntryQueryHandler(
            new TestCurrentUserAccessor { UserId = patientUserId },
            patientRepository.Object,
            healthRecordRepository.Object,
            entryRepository.Object,
            auditService.Object);

        await Assert.ThrowsAsync<AccessDeniedException>(
            () => handler.Handle(new GetPatientHealthRecordEntryQuery(hiddenEntry.Id), CancellationToken.None));

        auditService.Verify(
            x => x.LogPatientAccessAttemptAsync(
                patient.Id,
                healthRecord.Id,
                HealthRecordAccessOperations.GetPatientEntry,
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
