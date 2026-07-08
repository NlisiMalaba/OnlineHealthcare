using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Labs;
using HealthPlatform.Application.Labs.CreatePatientLabOrderRequest;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using Moq;
using Xunit;

namespace HealthPlatform.Tests.Unit.Labs;

public sealed class CreatePatientLabOrderRequestCommandHandlerTests
{
    [Fact]
    public async Task Handle_creates_pending_patient_request()
    {
        var currentUser = new Mock<ICurrentUserAccessor>();
        currentUser.SetupGet(x => x.UserId).Returns(Guid.CreateVersion7());

        var patientRepository = new Mock<IPatientRepository>();
        var healthRecordRepository = new Mock<IHealthRecordRepository>();
        var labOrderRepository = new Mock<ILabOrderRepository>();

        var patient = Patient.RegisterWithPhone(currentUser.Object.UserId!.Value, "Patient One", "+263700000099");
        var healthRecord = HealthRecord.CreateForPatient(patient.Id);

        patientRepository.Setup(x => x.GetByUserIdAsync(currentUser.Object.UserId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        healthRecordRepository.Setup(x => x.GetByPatientIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthRecord);

        var handler = new CreatePatientLabOrderRequestCommandHandler(
            currentUser.Object,
            patientRepository.Object,
            healthRecordRepository.Object,
            labOrderRepository.Object,
            TimeProvider.System);

        var result = await handler.Handle(
            new CreatePatientLabOrderRequestCommand("labx", "cbc", "Please review"),
            CancellationToken.None);

        Assert.Equal(patient.Id, result.PatientId);
        Assert.Equal(healthRecord.Id, result.HealthRecordId);
        Assert.Equal("LABX", result.LabPartnerCode);
        Assert.Equal("CBC", result.TestCode);
        Assert.Equal(HealthPlatform.Domain.Labs.LabOrderStatus.PendingDoctorApproval, result.Status);
        labOrderRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
