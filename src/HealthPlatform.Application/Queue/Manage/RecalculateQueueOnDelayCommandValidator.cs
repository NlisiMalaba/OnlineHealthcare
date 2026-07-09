using FluentValidation;

namespace HealthPlatform.Application.Queue.Manage;

public sealed class RecalculateQueueOnDelayCommandValidator : AbstractValidator<RecalculateQueueOnDelayCommand>
{
    public RecalculateQueueOnDelayCommandValidator()
    {
        RuleFor(x => x.DelayMinutes)
            .GreaterThan(QueuePolicies.DelayRecalculationThresholdMinutes);
    }
}
