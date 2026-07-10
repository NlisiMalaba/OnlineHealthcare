using FluentValidation;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.ListMoodLogs;

public sealed class ListMoodLogsQueryValidator : AbstractValidator<ListMoodLogsQuery>
{
    public ListMoodLogsQueryValidator()
    {
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
