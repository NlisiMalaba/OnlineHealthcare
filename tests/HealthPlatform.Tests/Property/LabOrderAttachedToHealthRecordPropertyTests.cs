using FsCheck.Xunit;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Labs;
using HealthPlatform.Application.Labs.CreateDoctorLabOrder;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Labs;
using HealthPlatform.Domain.ValueObjects;
using HealthPlatform.Infrastructure.MongoDb;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Moq;

namespace HealthPlatform.Tests.Properties;

public sealed class LabOrderAttachedToHealthRecordPropertyTests
{
    // Feature: online-healthcare-platform, Property 32: Lab Order Attached to Health Record
    [Property(Arbitrary = [typeof(LabOrderAttachmentArbitraries)], MaxTest = 100)]
    public bool Doctor_created_lab_order_is_always_attached_and_queryable_in_health_record(
        LabOrderAttachmentCase input) =>
        RunInvariantAsync(input).GetAwaiter().GetResult();

    private static async Task<bool> RunInvariantAsync(LabOrderAttachmentCase input)
    {
        var nowUtc = new DateTime(2026, 7, 7, 8, 0, 0, DateTimeKind.Utc);
        var timeProvider = new FakeTimeProvider(nowUtc);
        var patient = Patient.RegisterWithEmail(Guid.CreateVersion7(), "Property Patient", "property32@example.com");
        var healthRecord = HealthRecord.CreateForPatient(patient.Id);
        var doctorUserId = Guid.CreateVersion7();
        var doctor = CreateVerifiedDoctor(doctorUserId);

        var currentUser = new Mock<ICurrentUserAccessor>();
        currentUser.SetupGet(x => x.UserId).Returns(doctorUserId);

        var doctorRepository = new Mock<IDoctorRepository>();
        doctorRepository.Setup(x => x.GetByUserIdWithSlotsAsync(doctorUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);

        var patientRepository = new Mock<IPatientRepository>();
        patientRepository.Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        var healthRecordRepository = new Mock<IHealthRecordRepository>();
        healthRecordRepository.Setup(x => x.GetByIdAsync(healthRecord.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthRecord);

        var healthRecordEntryRepository = new InMemoryHealthRecordEntryRepository();
        var labOrderRepository = new InMemoryLabOrderRepository();
        var partnerClient = new StubLabPartnerOrderClient();

        var handler = new CreateDoctorLabOrderCommandHandler(
            currentUser.Object,
            doctorRepository.Object,
            patientRepository.Object,
            healthRecordRepository.Object,
            healthRecordEntryRepository,
            labOrderRepository,
            partnerClient,
            timeProvider);

        var created = await handler.Handle(
            new CreateDoctorLabOrderCommand(
                patient.Id,
                healthRecord.Id,
                input.LabPartnerCode,
                input.TestCode,
                input.ClinicalNotes),
            CancellationToken.None);

        var persistedOrder = await labOrderRepository.GetByIdAsync(created.Id, CancellationToken.None);
        if (persistedOrder is null || persistedOrder.Status != LabOrderStatus.SubmittedToLabPartner)
        {
            return false;
        }

        var entries = await healthRecordEntryRepository.ListByHealthRecordIdAsync(
            healthRecord.Id,
            patientVisibleOnly: true,
            CancellationToken.None);
        var labEntry = entries.SingleOrDefault(e => e.EntryType == HealthRecordEntryType.LabOrderRef);
        if (labEntry?.Content.LabOrderRef is null)
        {
            return false;
        }

        return labEntry.Content.LabOrderRef.LabOrderId == created.Id
               && labEntry.Content.LabOrderRef.TestCode == input.TestCode.Trim().ToUpperInvariant()
               && labEntry.Content.LabOrderRef.LabPartnerCode == input.LabPartnerCode.Trim().ToUpperInvariant();
    }

    private static Doctor CreateVerifiedDoctor(Guid userId)
    {
        var doctorId = Guid.CreateVersion7();
        var doctor = Doctor.Register(
            doctorId,
            userId,
            "Dr. Property",
            "LIC-PROP-32",
            "General",
            9,
            "Property Clinic",
            new GeoPoint(-17.8, 31.0),
            45m,
            70m,
            "Property doctor profile",
            "doctor-property@example.com",
            "+263771234567",
            null,
            null,
            [DoctorAvailabilitySlot.Create(
                doctorId,
                DayOfWeek.Monday,
                new TimeOnly(9, 0),
                new TimeOnly(11, 0),
                30,
                DoctorAppointmentType.Both)]);
        doctor.VerifyLicense();
        return doctor;
    }

    private sealed class StubLabPartnerOrderClient : ILabPartnerOrderClient
    {
        public Task<string> SubmitOrderAsync(LabPartnerOrderSubmission submission, CancellationToken ct) =>
            Task.FromResult($"REF-{submission.LabOrderId:N}");
    }

    private sealed class InMemoryLabOrderRepository : ILabOrderRepository
    {
        private readonly Dictionary<Guid, LabOrder> _orders = [];

        public Task AddAsync(LabOrder order, CancellationToken ct)
        {
            _orders[order.Id] = order;
            return Task.CompletedTask;
        }

        public Task<LabOrder?> GetByIdAsync(Guid labOrderId, CancellationToken ct)
        {
            _orders.TryGetValue(labOrderId, out var order);
            return Task.FromResult(order);
        }

        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
