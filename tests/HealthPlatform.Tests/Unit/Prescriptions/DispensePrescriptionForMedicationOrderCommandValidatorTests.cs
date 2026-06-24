using FluentValidation.TestHelper;
using HealthPlatform.Application.Prescriptions.Dispensing;
using Xunit;

namespace HealthPlatform.Tests.Unit.Prescriptions;

public sealed class DispensePrescriptionForMedicationOrderCommandValidatorTests
{
    private readonly DispensePrescriptionForMedicationOrderCommandValidator _validator = new();

    [Fact]
    public void Empty_prescription_id_fails_validation()
    {
        var result = _validator.TestValidate(new DispensePrescriptionForMedicationOrderCommand(Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.PrescriptionId);
    }
}
