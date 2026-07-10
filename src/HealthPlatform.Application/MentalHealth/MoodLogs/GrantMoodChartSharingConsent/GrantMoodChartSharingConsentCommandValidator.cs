using FluentValidation;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.GrantMoodChartSharingConsent;

public sealed class GrantMoodChartSharingConsentCommandValidator
    : AbstractValidator<GrantMoodChartSharingConsentCommand>
{
    public GrantMoodChartSharingConsentCommandValidator()
    {
        RuleFor(command => command.TherapistId)
            .NotEmpty();
    }
}
