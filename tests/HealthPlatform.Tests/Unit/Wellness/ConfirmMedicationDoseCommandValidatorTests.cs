using HealthPlatform.Application.Wellness;
using HealthPlatform.Application.Wellness.ConfirmMedicationDose;
using FluentValidation.TestHelper;
using Xunit;

namespace HealthPlatform.Tests.Unit.Wellness;

public sealed class ConfirmMedicationDoseCommandValidatorTests
{
    private readonly ConfirmMedicationDoseCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes_validation()
    {
        var result = _validator.TestValidate(new ConfirmMedicationDoseCommand(
            Guid.CreateVersion7(),
            new DateTime(2026, 6, 24, 8, 0, 0, DateTimeKind.Utc)));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Missing_schedule_id_fails_validation()
    {
        var result = _validator.TestValidate(new ConfirmMedicationDoseCommand(
            Guid.Empty,
            new DateTime(2026, 6, 24, 8, 0, 0, DateTimeKind.Utc)));

        result.ShouldHaveValidationErrorFor(command => command.ScheduleId);
    }

    [Fact]
    public void Non_utc_scheduled_time_fails_validation()
    {
        var result = _validator.TestValidate(new ConfirmMedicationDoseCommand(
            Guid.CreateVersion7(),
            new DateTime(2026, 6, 24, 8, 0, 0, DateTimeKind.Local)));

        result.ShouldHaveValidationErrorFor(command => command.ScheduledAtUtc);
    }
}
