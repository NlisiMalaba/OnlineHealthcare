using FluentValidation;

namespace HealthPlatform.Application.Wellness.CarePlans.AssignCarePlan;

public sealed class AssignCarePlanCommandValidator : AbstractValidator<AssignCarePlanCommand>
{
    public AssignCarePlanCommandValidator()
    {
        RuleFor(command => command.PatientId).NotEmpty();
        RuleFor(command => command.Condition)
            .NotEmpty()
            .MaximumLength(WellnessPolicies.MaxCarePlanConditionLength);
        RuleFor(command => command.Tasks)
            .NotEmpty()
            .Must(tasks => tasks.Count <= WellnessPolicies.MaxCarePlanTasks)
            .WithMessage($"Care plan cannot exceed {WellnessPolicies.MaxCarePlanTasks} tasks.");
        RuleForEach(command => command.Tasks).ChildRules(task =>
        {
            task.RuleFor(t => t.Title)
                .NotEmpty()
                .MaximumLength(WellnessPolicies.MaxCarePlanTaskTitleLength);
            task.RuleFor(t => t.Description)
                .MaximumLength(WellnessPolicies.MaxCarePlanTaskDescriptionLength)
                .When(t => t.Description is not null);
            task.RuleFor(t => t.DueDate).NotEmpty();
        });
        RuleFor(command => command.MonitoringTargets)
            .NotEmpty()
            .Must(targets => targets.Count <= WellnessPolicies.MaxCarePlanMonitoringTargets)
            .WithMessage(
                $"Care plan cannot exceed {WellnessPolicies.MaxCarePlanMonitoringTargets} monitoring targets.");
        RuleForEach(command => command.MonitoringTargets).ChildRules(target =>
        {
            target.RuleFor(t => t.MetricName)
                .NotEmpty()
                .MaximumLength(WellnessPolicies.MaxCarePlanMetricNameLength);
            target.RuleFor(t => t.TargetValue)
                .InclusiveBetween(WellnessPolicies.MinTargetValue, WellnessPolicies.MaxTargetValue);
            target.RuleFor(t => t.Unit)
                .NotEmpty()
                .MaximumLength(WellnessPolicies.MaxUnitLength);
        });
        RuleFor(command => command.ReviewIntervalDays)
            .InclusiveBetween(
                WellnessPolicies.MinCarePlanReviewIntervalDays,
                WellnessPolicies.MaxCarePlanReviewIntervalDays);
    }
}
