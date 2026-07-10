using FluentValidation;
using HealthPlatform.Domain.Maternal;

namespace HealthPlatform.Application.Maternal.AntenatalRecords.ConfigureFetalMonitoringReminders;

public sealed class ConfigureFetalMonitoringRemindersCommandValidator
    : AbstractValidator<ConfigureFetalMonitoringRemindersCommand>
{
    public ConfigureFetalMonitoringRemindersCommandValidator()
    {
        RuleFor(command => command.AntenatalRecordId)
            .NotEmpty();

        RuleFor(command => command.IntervalDays)
            .Must(FetalMonitoringReminderPolicies.IsValidIntervalDays)
            .WithMessage(
                $"Fetal monitoring reminder interval must be between {FetalMonitoringReminderPolicies.MinIntervalDays} and {FetalMonitoringReminderPolicies.MaxIntervalDays} days.");
    }
}
