using HealthPlatform.Application.Prescriptions.CreatePrescription;
using Xunit;

namespace HealthPlatform.Tests.Unit.Prescriptions;

public sealed class CreatePrescriptionCommandValidatorTests
{
    private readonly CreatePrescriptionCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes_validation()
    {
        var result = _validator.Validate(new CreatePrescriptionCommand(
            Guid.CreateVersion7(),
            "Amoxicillin",
            "500mg",
            "Twice daily",
            7,
            "Take with food",
            null,
            null));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Missing_medication_name_fails_validation()
    {
        var result = _validator.Validate(new CreatePrescriptionCommand(
            Guid.CreateVersion7(),
            "",
            "500mg",
            "Twice daily",
            7,
            null,
            null,
            null));

        Assert.False(result.IsValid);
    }
}
