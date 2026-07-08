using HealthPlatform.Application.Labs.ApprovePatientLabOrder;
using HealthPlatform.Application.Labs.CreateDoctorLabOrder;
using HealthPlatform.Application.Labs.CreatePatientLabOrderRequest;
using Xunit;

namespace HealthPlatform.Tests.Unit.Labs;

public sealed class LabOrderCommandValidatorTests
{
    [Fact]
    public void Create_doctor_order_validator_rejects_missing_test_code()
    {
        var validator = new CreateDoctorLabOrderCommandValidator();
        var result = validator.Validate(new CreateDoctorLabOrderCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "LABX",
            string.Empty,
            null));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Create_patient_request_validator_accepts_valid_request()
    {
        var validator = new CreatePatientLabOrderRequestCommandValidator();
        var result = validator.Validate(new CreatePatientLabOrderRequestCommand("LABX", "CBC", "Fasting"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Approve_patient_order_validator_requires_lab_order_id()
    {
        var validator = new ApprovePatientLabOrderCommandValidator();
        var result = validator.Validate(new ApprovePatientLabOrderCommand(Guid.Empty));

        Assert.False(result.IsValid);
    }
}
