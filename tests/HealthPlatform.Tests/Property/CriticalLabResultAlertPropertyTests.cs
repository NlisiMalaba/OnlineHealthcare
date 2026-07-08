using FsCheck.Xunit;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Labs;
using HealthPlatform.Application.Labs.Webhooks;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Storage;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Labs;
using HealthPlatform.Domain.ValueObjects;
using HealthPlatform.Tests.Arbitraries;
using Moq;

namespace HealthPlatform.Tests.Properties;

public sealed class CriticalLabResultAlertPropertyTests
{
    // Feature: online-healthcare-platform, Property 33: Critical Lab Result Alert
    [Property(Arbitrary = [typeof(CriticalLabResultAlertArbitraries)], MaxTest = 100)]
    public bool Critical_lab_result_upload_immediately_alerts_ordering_doctor(
        CriticalLabResultAlertCase input) =>
        RunCriticalAlertInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunCriticalAlertInvariantAsync(CriticalLabResultAlertCase input)
    {
        var patient = Patient.RegisterWithEmail(Guid.CreateVersion7(), "Property Patient", "property33@example.com");
        var doctor = CreateVerifiedDoctor(Guid.CreateVersion7());
        var order = LabOrder.CreateDoctorOrdered(
            patient.Id,
            Guid.CreateVersion7(),
            doctor.Id,
            input.LabPartnerCode,
            input.TestCode,
            "urgent",
            DateTime.UtcNow);
        order.MarkSubmitted(input.LabPartnerOrderReference);

        var labOrderRepository = new Mock<ILabOrderRepository>();
        labOrderRepository
            .Setup(x => x.GetByPartnerReferenceAsync(input.LabPartnerCode, input.LabPartnerOrderReference, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var labResultRepository = new Mock<ILabResultRepository>();

        var patientRepository = new Mock<IPatientRepository>();
        patientRepository.Setup(x => x.GetByIdAsync(order.PatientId, It.IsAny<CancellationToken>())).ReturnsAsync(patient);

        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository.Setup(x => x.GetByIdAsync(order.OrderingDoctorId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        var storageService = new Mock<IStorageService>();
        storageService
            .Setup(x => x.UploadLabResultAsync(
                order.PatientId,
                order.Id,
                It.IsAny<Stream>(),
                "application/pdf",
                input.FileName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StorageUploadResult($"patients/{order.PatientId:N}/labs/{order.Id:N}/result.pdf", "application/pdf"));

        var healthRecordEntryRepository = new Mock<IHealthRecordEntryRepository>();
        healthRecordEntryRepository
            .Setup(x => x.AddAsync(It.IsAny<HealthRecordEntryCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HealthRecordEntryDto(
                Guid.CreateVersion7().ToString("N"),
                order.HealthRecordId,
                HealthRecordEntryType.LabResultRef,
                new HealthRecordEntryContentPayload(LabResultRef: new LabResultRefContent(Guid.CreateVersion7())),
                doctor.Id,
                DateTime.UtcNow,
                null,
                true));

        var notificationDispatcher = new Mock<INotificationDispatcher>();
        notificationDispatcher
            .Setup(x => x.DispatchAsync(It.IsAny<NotificationDispatchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationDispatchResult([]));

        var handler = new IngestLabResultWebhookCommandHandler(
            labOrderRepository.Object,
            labResultRepository.Object,
            patientRepository.Object,
            doctorRepository.Object,
            storageService.Object,
            healthRecordEntryRepository.Object,
            notificationDispatcher.Object,
            TimeProvider.System);

        await handler.Handle(
            new IngestLabResultWebhookCommand(
                input.LabPartnerCode,
                input.LabPartnerOrderReference,
                input.TestCode,
                [0x25, 0x50, 0x44, 0x46],
                "application/pdf",
                input.FileName,
                IsCritical: true),
            CancellationToken.None);

        notificationDispatcher.Verify(x => x.DispatchAsync(
            It.Is<NotificationDispatchRequest>(request =>
                request.UserId == doctor.UserId
                && request.RecipientType == NotificationRecipientType.Doctor
                && request.EventType == NotificationEventTypes.CriticalLabResultAlert
                && request.Criticality == NotificationCriticality.Critical),
            It.IsAny<CancellationToken>()), Times.Once);

        return true;
    }

    private static Doctor CreateVerifiedDoctor(Guid userId)
    {
        var doctorId = Guid.CreateVersion7();
        var doctor = Doctor.Register(
            doctorId,
            userId,
            "Dr. Critical Alert",
            "LIC-CRIT-33",
            "General",
            8,
            "Critical Care Clinic",
            new GeoPoint(-17.8, 31.0),
            40m,
            60m,
            "Bio",
            "critical-alert-doctor@example.com",
            "+263771234567",
            null,
            null,
            [DoctorAvailabilitySlot.Create(
                doctorId,
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(10, 0),
                30,
                DoctorAppointmentType.Both)]);
        doctor.VerifyLicense();
        return doctor;
    }
}
