using FluentValidation;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.RevokeMoodChartSharingConsent;

public sealed class RevokeMoodChartSharingConsentCommandValidator
    : AbstractValidator<RevokeMoodChartSharingConsentCommand>
{
    public RevokeMoodChartSharingConsentCommandValidator()
    {
        RuleFor(command => command.TherapistId)
            .NotEmpty();
    }
}
