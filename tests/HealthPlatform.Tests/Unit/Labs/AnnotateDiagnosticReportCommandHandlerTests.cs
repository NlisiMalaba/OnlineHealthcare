using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Labs;
using HealthPlatform.Application.Labs.AnnotateDiagnosticReport;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Labs;
using HealthPlatform.Domain.ValueObjects;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Labs;

public sealed class AnnotateDiagnosticReportCommandHandlerTests
{
    [Fact]
    public async Task Handle_creates_patient_visible_annotation_entry_for_lab_result()
    {
        var currentUser = new Mock<ICurrentUserAccessor>();
        var doctor = CreateVerifiedDoctor(Guid.CreateVersion7());
        currentUser.SetupGet(x => x.UserId).Returns(doctor.UserId);

        var labResult = LabResult.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            doctor.Id,
            "LABX",
            "REF-1",
            "CBC",
            "patients/x/lab.pdf",
            "application/pdf",
            "lab.pdf",
            false);

        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository.Setup(x => x.GetByUserIdWithSlotsAsync(doctor.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        var labResultRepository = new Mock<ILabResultRepository>();
        labResultRepository.Setup(x => x.GetByIdAsync(labResult.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(labResult);

        var radiologyRepository = new Mock<IRadiologyReportRepository>();

        var entryRepository = new Mock<IHealthRecordEntryRepository>();
        entryRepository.Setup(x => x.AddAsync(It.IsAny<HealthRecordEntryCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HealthRecordEntryDto(
                "entry-annot",
                labResult.HealthRecordId,
                HealthRecordEntryType.DiagnosticReportAnnotation,
                new HealthRecordEntryContentPayload(
                    DiagnosticReportAnnotation: new DiagnosticReportAnnotationContent(
                        "LabResult",
                        labResult.Id,
                        "Review note",
                        DateTime.UtcNow)),
                doctor.Id,
                DateTime.UtcNow,
                null,
                true));

        var accessGuard = new Mock<IHealthRecordAccessGuard>();

        var handler = new AnnotateDiagnosticReportCommandHandler(
            currentUser.Object,
            doctorRepository.Object,
            labResultRepository.Object,
            radiologyRepository.Object,
            entryRepository.Object,
            accessGuard.Object,
            TimeProvider.System);

        var response = await handler.Handle(
            new AnnotateDiagnosticReportCommand(DiagnosticAnnotationTargetType.LabResult, labResult.Id, "Review note"),
            CancellationToken.None);

        Assert.Equal(HealthRecordEntryType.DiagnosticReportAnnotation, response.EntryType);
        Assert.True(response.IsVisibleToPatient);
        entryRepository.Verify(
            x => x.AddAsync(
                It.Is<HealthRecordEntryCreateModel>(m =>
                    m.EntryType == HealthRecordEntryType.DiagnosticReportAnnotation
                    && m.IsVisibleToPatient
                    && m.Content.DiagnosticReportAnnotation!.Note == "Review note"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_creates_patient_visible_annotation_entry_for_radiology_report()
    {
        var currentUser = new Mock<ICurrentUserAccessor>();
        var doctor = CreateVerifiedDoctor(Guid.CreateVersion7());
        currentUser.SetupGet(x => x.UserId).Returns(doctor.UserId);

        var report = RadiologyReport.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            doctor.Id,
            "LABX",
            "REF-RAD-1",
            "patients/x/report.pdf",
            "application/pdf",
            "report.pdf",
            Array.Empty<string>());

        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository.Setup(x => x.GetByUserIdWithSlotsAsync(doctor.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        var labResultRepository = new Mock<ILabResultRepository>();
        var radiologyRepository = new Mock<IRadiologyReportRepository>();
        radiologyRepository.Setup(x => x.GetByIdAsync(report.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var entryRepository = new Mock<IHealthRecordEntryRepository>();
        entryRepository.Setup(x => x.AddAsync(It.IsAny<HealthRecordEntryCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HealthRecordEntryDto(
                "entry-rad-annot",
                report.HealthRecordId,
                HealthRecordEntryType.DiagnosticReportAnnotation,
                new HealthRecordEntryContentPayload(
                    DiagnosticReportAnnotation: new DiagnosticReportAnnotationContent(
                        "RadiologyReport",
                        report.Id,
                        "Stable findings",
                        DateTime.UtcNow)),
                doctor.Id,
                DateTime.UtcNow,
                null,
                true));

        var accessGuard = new Mock<IHealthRecordAccessGuard>();

        var handler = new AnnotateDiagnosticReportCommandHandler(
            currentUser.Object,
            doctorRepository.Object,
            labResultRepository.Object,
            radiologyRepository.Object,
            entryRepository.Object,
            accessGuard.Object,
            TimeProvider.System);

        var response = await handler.Handle(
            new AnnotateDiagnosticReportCommand(
                DiagnosticAnnotationTargetType.RadiologyReport,
                report.Id,
                "Stable findings"),
            CancellationToken.None);

        Assert.Equal(HealthRecordEntryType.DiagnosticReportAnnotation, response.EntryType);
        Assert.True(response.IsVisibleToPatient);
        accessGuard.Verify(
            x => x.EnsureDoctorCanReadAsync(
                report.HealthRecordId,
                doctor.Id,
                HealthRecordAccessOperations.AnnotateDiagnosticReport,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Doctor CreateVerifiedDoctor(Guid userId)
    {
        var doctorId = Guid.CreateVersion7();
        var doctor = Doctor.Register(
            doctorId,
            userId,
            "Dr. Annotator",
            "LIC-ANNOTATE-1",
            "Radiology",
            7,
            "Clinic",
            new GeoPoint(-17.8, 31.0),
            30m,
            50m,
            "Bio",
            "annotator@example.com",
            "+263771234567",
            null,
            null,
            [DoctorAvailabilitySlot.Create(doctorId, DayOfWeek.Tuesday, new TimeOnly(9, 0), new TimeOnly(10, 0), 30, DoctorAppointmentType.Both)]);
        doctor.VerifyLicense();
        return doctor;
    }
}
