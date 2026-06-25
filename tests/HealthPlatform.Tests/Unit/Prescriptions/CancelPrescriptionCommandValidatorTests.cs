using FluentValidation.TestHelper;
using HealthPlatform.Application.Prescriptions.CancelPrescription;
using Xunit;

namespace HealthPlatform.Tests.Unit.Prescriptions;

public sealed class CancelPrescriptionCommandValidatorTests
{
    private readonly CancelPrescriptionCommandValidator _validator = new();

    [Fact]
    public void Missing_reason_fails_validation()
    {
        var result = _validator.TestValidate(new CancelPrescriptionCommand(Guid.CreateVersion7(), ""));
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }
}
