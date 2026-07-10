using FluentValidation;

namespace HealthPlatform.Application.MentalHealth.MoodLogs.GetMoodLog;

public sealed class GetMoodLogQueryValidator : AbstractValidator<GetMoodLogQuery>
{
    public GetMoodLogQueryValidator()
    {
        RuleFor(query => query.MoodLogId)
            .NotEmpty();
    }
}
