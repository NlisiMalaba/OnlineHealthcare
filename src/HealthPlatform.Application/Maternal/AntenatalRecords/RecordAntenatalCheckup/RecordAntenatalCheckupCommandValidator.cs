using FluentValidation;
using HealthPlatform.Domain.Maternal;

namespace HealthPlatform.Application.Maternal.AntenatalRecords.RecordAntenatalCheckup;

public sealed class RecordAntenatalCheckupCommandValidator : AbstractValidator<RecordAntenatalCheckupCommand>
{
    public RecordAntenatalCheckupCommandValidator()
    {
        RuleFor(command => command.AntenatalRecordId)
            .NotEmpty();

        RuleFor(command => command.GestationalAgeWeeks)
            .InclusiveBetween(AntenatalCheckupPolicies.MinGestationalAgeWeeks, AntenatalCheckupPolicies.MaxGestationalAgeWeeks);

        RuleFor(command => command.FetalHeartRateBpm)
            .InclusiveBetween(AntenatalCheckupPolicies.MinFetalHeartRateBpm, AntenatalCheckupPolicies.MaxFetalHeartRateBpm)
            .When(command => command.FetalHeartRateBpm.HasValue);

        RuleFor(command => command.FundalHeightCm)
            .GreaterThan(0)
            .When(command => command.FundalHeightCm.HasValue);

        RuleFor(command => command.EstimatedFetalWeightGrams)
            .GreaterThan(0)
            .When(command => command.EstimatedFetalWeightGrams.HasValue);

        RuleFor(command => command.BloodPressureSystolic)
            .InclusiveBetween(
                AntenatalCheckupPolicies.MinBloodPressureSystolic,
                AntenatalCheckupPolicies.MaxBloodPressureSystolic)
            .When(command => command.BloodPressureSystolic.HasValue);

        RuleFor(command => command.BloodPressureDiastolic)
            .InclusiveBetween(
                AntenatalCheckupPolicies.MinBloodPressureDiastolic,
                AntenatalCheckupPolicies.MaxBloodPressureDiastolic)
            .When(command => command.BloodPressureDiastolic.HasValue);

        RuleFor(command => command.MaternalWeightKg)
            .GreaterThan(0)
            .When(command => command.MaternalWeightKg.HasValue);

        RuleFor(command => command.ClinicalNotes)
            .MaximumLength(AntenatalCheckupPolicies.MaxClinicalNotesLength)
            .When(command => !string.IsNullOrWhiteSpace(command.ClinicalNotes));

        RuleFor(command => command.FetalMonitoringReminderIntervalDays)
            .Must(interval => !interval.HasValue || FetalMonitoringReminderPolicies.IsValidIntervalDays(interval.Value))
            .WithMessage(
                $"Fetal monitoring reminder interval must be between {FetalMonitoringReminderPolicies.MinIntervalDays} and {FetalMonitoringReminderPolicies.MaxIntervalDays} days.");
    }
}
