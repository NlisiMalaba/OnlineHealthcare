using FluentValidation.TestHelper;
using HealthPlatform.Application.Maternal.AntenatalRecords.ConfigureFetalMonitoringReminders;
using HealthPlatform.Application.Maternal.AntenatalRecords.RecordAntenatalCheckup;
using Xunit;

namespace HealthPlatform.Tests.Unit.Maternal;

public sealed class RecordAntenatalCheckupCommandValidatorTests
{
    private readonly RecordAntenatalCheckupCommandValidator _recordValidator = new();
    private readonly ConfigureFetalMonitoringRemindersCommandValidator _configureValidator = new();

    [Fact]
    public void Valid_record_command_passes_validation()
    {
        var result = _recordValidator.TestValidate(new RecordAntenatalCheckupCommand(
            Guid.CreateVersion7(),
            null,
            20,
            140,
            20.5m,
            3200m,
            118,
            76,
            68.5m,
            "Routine antenatal visit.",
            3));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Invalid_fetal_monitoring_interval_fails_validation()
    {
        var result = _recordValidator.TestValidate(new RecordAntenatalCheckupCommand(
            Guid.CreateVersion7(),
            null,
            20,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            20));

        result.ShouldHaveValidationErrorFor(command => command.FetalMonitoringReminderIntervalDays);
    }

    [Fact]
    public void Configure_command_validates_interval_days()
    {
        var valid = _configureValidator.TestValidate(
            new ConfigureFetalMonitoringRemindersCommand(Guid.CreateVersion7(), 7));
        valid.ShouldNotHaveAnyValidationErrors();

        var invalid = _configureValidator.TestValidate(
            new ConfigureFetalMonitoringRemindersCommand(Guid.CreateVersion7(), 0));
        invalid.ShouldHaveValidationErrorFor(command => command.IntervalDays);
    }
}
