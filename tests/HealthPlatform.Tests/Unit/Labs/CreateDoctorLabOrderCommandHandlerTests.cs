using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Labs;
using HealthPlatform.Application.Labs.CreateDoctorLabOrder;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.ValueObjects;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Labs;

public sealed class CreateDoctorLabOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_submits_order_and_attaches_health_record_entry()
    {
        var currentUser = new Mock<ICurrentUserAccessor>();
        currentUser.SetupGet(x => x.UserId).Returns(Guid.CreateVersion7());

        var doctorRepository = new Mock<IDoctorRepository>();
        var patientRepository = new Mock<IPatientRepository>();
        var healthRecordRepository = new Mock<IHealthRecordRepository>();
        var healthRecordEntryRepository = new Mock<IHealthRecordEntryRepository>();
        var labOrderRepository = new Mock<ILabOrderRepository>();
        var partnerClient = new Mock<ILabPartnerOrderClient>();

        var patient = Patient.RegisterWithEmail(Guid.CreateVersion7(), "Patient One", "patient@example.com");
        var healthRecord = HealthRecord.CreateForPatient(patient.Id);
        var doctor = CreateVerifiedDoctor(currentUser.Object.UserId!.Value);

        doctorRepository.Setup(x => x.GetByUserIdWithSlotsAsync(currentUser.Object.UserId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        patientRepository.Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        healthRecordRepository.Setup(x => x.GetByIdAsync(healthRecord.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthRecord);
        partnerClient.Setup(x => x.SubmitOrderAsync(It.IsAny<LabPartnerOrderSubmission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("LAB-REF-1001");
        healthRecordEntryRepository.Setup(x => x.AddAsync(It.IsAny<HealthRecordEntryCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HealthRecordEntryDto(
                "entry-1",
                healthRecord.Id,
                HealthRecordEntryType.LabOrderRef,
                new HealthRecordEntryContentPayload(LabOrderRef: new LabOrderRefContent(Guid.CreateVersion7(), "CBC", "LABX")),
                doctor.Id,
                DateTime.UtcNow,
                null,
                true));

        var handler = new CreateDoctorLabOrderCommandHandler(
            currentUser.Object,
            doctorRepository.Object,
            patientRepository.Object,
            healthRecordRepository.Object,
            healthRecordEntryRepository.Object,
            labOrderRepository.Object,
            partnerClient.Object,
            TimeProvider.System);

        var result = await handler.Handle(
            new CreateDoctorLabOrderCommand(patient.Id, healthRecord.Id, "labx", "cbc", "Fasting"),
            CancellationToken.None);

        Assert.Equal("LABX", result.LabPartnerCode);
        Assert.Equal("CBC", result.TestCode);
        Assert.Equal(doctor.Id, result.OrderingDoctorId);
        Assert.Equal("LAB-REF-1001", result.LabPartnerOrderReference);
        healthRecordEntryRepository.Verify(x => x.AddAsync(
            It.Is<HealthRecordEntryCreateModel>(m => m.EntryType == HealthRecordEntryType.LabOrderRef),
            It.IsAny<CancellationToken>()), Times.Once);
        labOrderRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Doctor CreateVerifiedDoctor(Guid userId)
    {
        var doctorId = Guid.CreateVersion7();
        var doctor = Doctor.Register(
            doctorId,
            userId,
            "Dr. Verified",
            "LIC-1000",
            "General",
            10,
            "Clinic Street 1",
            new GeoPoint(-17.0, 31.0),
            40m,
            60m,
            "Bio",
            "doctor@example.com",
            "+263700000001",
            null,
            null,
            [DoctorAvailabilitySlot.Create(doctorId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(10, 0), 30, DoctorAppointmentType.Both)]);
        doctor.VerifyLicense();
        return doctor;
    }
}
