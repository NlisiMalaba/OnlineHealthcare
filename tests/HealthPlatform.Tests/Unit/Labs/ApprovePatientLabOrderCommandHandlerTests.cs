using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Labs;
using HealthPlatform.Application.Labs.ApprovePatientLabOrder;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Labs;
using HealthPlatform.Domain.ValueObjects;
using HealthPlatform.Tests.Support;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Labs;

public sealed class ApprovePatientLabOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_approves_pending_patient_request_submits_to_partner_and_attaches_health_record_entry()
    {
        var doctorUserId = Guid.CreateVersion7();
        var currentUser = new Mock<ICurrentUserAccessor>();
        currentUser.SetupGet(x => x.UserId).Returns(doctorUserId);

        var doctor = CreateVerifiedDoctor(doctorUserId);
        var patient = Patient.RegisterWithEmail(Guid.CreateVersion7(), "Patient One", "patient@example.com");
        var healthRecord = HealthRecord.CreateForPatient(patient.Id);
        var order = LabOrder.CreatePatientRequested(
            patient.Id,
            healthRecord.Id,
            "labx",
            "cbc",
            "Please review",
            DateTime.UtcNow);

        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository.Setup(x => x.GetByUserIdWithSlotsAsync(doctorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        var labOrderRepository = new Mock<ILabOrderRepository>();
        labOrderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var partnerClient = new Mock<ILabPartnerOrderClient>();
        partnerClient.Setup(x => x.SubmitOrderAsync(It.IsAny<LabPartnerOrderSubmission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("LAB-REF-2001");

        var healthRecordEntryRepository = new Mock<IHealthRecordEntryRepository>();
        healthRecordEntryRepository.Setup(x => x.AddAsync(It.IsAny<HealthRecordEntryCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HealthRecordEntryDto(
                "entry-approve",
                healthRecord.Id,
                HealthRecordEntryType.LabOrderRef,
                new HealthRecordEntryContentPayload(
                    LabOrderRef: new LabOrderRefContent(order.Id, "CBC", "LABX")),
                doctor.Id,
                DateTime.UtcNow,
                null,
                true));

        var approvedAt = new DateTime(2026, 7, 7, 8, 0, 0, DateTimeKind.Utc);
        var handler = new ApprovePatientLabOrderCommandHandler(
            currentUser.Object,
            doctorRepository.Object,
            labOrderRepository.Object,
            healthRecordEntryRepository.Object,
            partnerClient.Object,
            new FakeTimeProvider(approvedAt));

        var result = await handler.Handle(new ApprovePatientLabOrderCommand(order.Id), CancellationToken.None);

        Assert.Equal(doctor.Id, result.OrderingDoctorId);
        Assert.Equal(LabOrderStatus.SubmittedToLabPartner, result.Status);
        Assert.Equal("LAB-REF-2001", result.LabPartnerOrderReference);
        Assert.Equal(approvedAt, result.ApprovedAtUtc);

        partnerClient.Verify(
            x => x.SubmitOrderAsync(
                It.Is<LabPartnerOrderSubmission>(s =>
                    s.LabOrderId == order.Id
                    && s.PatientId == patient.Id
                    && s.LabPartnerCode == "LABX"
                    && s.TestCode == "CBC"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        healthRecordEntryRepository.Verify(
            x => x.AddAsync(
                It.Is<HealthRecordEntryCreateModel>(m =>
                    m.HealthRecordId == healthRecord.Id
                    && m.EntryType == HealthRecordEntryType.LabOrderRef
                    && m.IsVisibleToPatient),
                It.IsAny<CancellationToken>()),
            Times.Once);

        labOrderRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_throws_when_order_is_not_pending_patient_approval()
    {
        var doctorUserId = Guid.CreateVersion7();
        var currentUser = new Mock<ICurrentUserAccessor>();
        currentUser.SetupGet(x => x.UserId).Returns(doctorUserId);

        var doctor = CreateVerifiedDoctor(doctorUserId);
        var patient = Patient.RegisterWithEmail(Guid.CreateVersion7(), "Patient One", "patient@example.com");
        var healthRecord = HealthRecord.CreateForPatient(patient.Id);
        var order = LabOrder.CreateDoctorOrdered(
            patient.Id,
            healthRecord.Id,
            doctor.Id,
            "labx",
            "cbc",
            null,
            DateTime.UtcNow);

        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository.Setup(x => x.GetByUserIdWithSlotsAsync(doctorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        var labOrderRepository = new Mock<ILabOrderRepository>();
        labOrderRepository.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var handler = new ApprovePatientLabOrderCommandHandler(
            currentUser.Object,
            doctorRepository.Object,
            labOrderRepository.Object,
            Mock.Of<IHealthRecordEntryRepository>(),
            Mock.Of<ILabPartnerOrderClient>(),
            TimeProvider.System);

        await Assert.ThrowsAsync<LabOrderApprovalNotAllowedException>(
            () => handler.Handle(new ApprovePatientLabOrderCommand(order.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_throws_when_doctor_is_not_verified()
    {
        var doctorUserId = Guid.CreateVersion7();
        var currentUser = new Mock<ICurrentUserAccessor>();
        currentUser.SetupGet(x => x.UserId).Returns(doctorUserId);

        var doctorId = Guid.CreateVersion7();
        var doctor = Doctor.Register(
            doctorId,
            doctorUserId,
            "Dr. Pending",
            "LIC-PENDING",
            "General",
            10,
            "Clinic",
            new GeoPoint(-17.0, 31.0),
            40m,
            60m,
            "Bio",
            "pending@example.com",
            "+263700000002",
            null,
            null,
            [DoctorAvailabilitySlot.Create(doctorId, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(10, 0), 30, DoctorAppointmentType.Both)]);

        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository.Setup(x => x.GetByUserIdWithSlotsAsync(doctorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        var handler = new ApprovePatientLabOrderCommandHandler(
            currentUser.Object,
            doctorRepository.Object,
            Mock.Of<ILabOrderRepository>(),
            Mock.Of<IHealthRecordEntryRepository>(),
            Mock.Of<ILabPartnerOrderClient>(),
            TimeProvider.System);

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new ApprovePatientLabOrderCommand(Guid.CreateVersion7()), CancellationToken.None));

        Assert.Equal(LabOrderErrorCodes.DoctorNotVerified, exception.Code);
    }

    private static Doctor CreateVerifiedDoctor(Guid userId)
    {
        var doctorId = Guid.CreateVersion7();
        var doctor = Doctor.Register(
            doctorId,
            userId,
            "Dr. Verified",
            "LIC-APPROVE",
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
