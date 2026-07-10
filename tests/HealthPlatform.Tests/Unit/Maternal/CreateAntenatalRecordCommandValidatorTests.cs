using FluentValidation.TestHelper;
using HealthPlatform.Application.Maternal.AntenatalRecords.CreateAntenatalRecord;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class CreateAntenatalRecordCommandValidatorTests
{
    private readonly CreateAntenatalRecordCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes_validation()
    {
        var result = _validator.TestValidate(new CreateAntenatalRecordCommand(
            new DateOnly(2026, 12, 1),
            20,
            Guid.CreateVersion7()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(43)]
    public void Gestational_age_outside_range_fails_validation(int gestationalAgeWeeks)
    {
        var result = _validator.TestValidate(new CreateAntenatalRecordCommand(
            new DateOnly(2026, 12, 1),
            gestationalAgeWeeks,
            Guid.CreateVersion7()));

        result.ShouldHaveValidationErrorFor(command => command.GestationalAgeWeeks);
    }

    [Fact]
    public void Missing_obstetric_doctor_fails_validation()
    {
        var result = _validator.TestValidate(new CreateAntenatalRecordCommand(
            new DateOnly(2026, 12, 1),
            20,
            Guid.Empty));

        result.ShouldHaveValidationErrorFor(command => command.ObstetricDoctorId);
    }
}
