using FluentValidation.TestHelper;
using HealthPlatform.Application.HealthRecords.RevokeHealthRecordAccess;
using Xunit;

namespace HealthPlatform.Tests.Unit.HealthRecords;

public sealed class RevokeHealthRecordAccessCommandValidatorTests
{
    private readonly RevokeHealthRecordAccessCommandValidator _validator = new();

    [Fact]
    public void Validator_accepts_valid_doctor_id()
    {
        var result = _validator.TestValidate(new RevokeHealthRecordAccessCommand(Guid.CreateVersion7()));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_rejects_empty_doctor_id()
    {
        var result = _validator.TestValidate(new RevokeHealthRecordAccessCommand(Guid.Empty));
        result.ShouldHaveValidationErrorFor(command => command.DoctorId);
    }
}
