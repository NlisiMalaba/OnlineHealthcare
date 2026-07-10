using FluentValidation;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.GetPatientMoodChartData;

public sealed class GetPatientMoodChartDataQueryValidator : AbstractValidator<GetPatientMoodChartDataQuery>
{
    public GetPatientMoodChartDataQueryValidator()
    {
        RuleFor(query => query.PatientId)
            .NotEmpty();

        RuleFor(query => query.FromUtc)
            .Must(fromUtc => !fromUtc.HasValue || fromUtc.Value.Kind == DateTimeKind.Utc)
            .WithMessage("From time must be in UTC.");

        RuleFor(query => query.ToUtc)
            .Must(toUtc => !toUtc.HasValue || toUtc.Value.Kind == DateTimeKind.Utc)
            .WithMessage("To time must be in UTC.");

        RuleFor(query => query)
            .Must(query => !query.FromUtc.HasValue || !query.ToUtc.HasValue || query.FromUtc <= query.ToUtc)
            .WithMessage("From time must be before or equal to to time.");
    }
}
