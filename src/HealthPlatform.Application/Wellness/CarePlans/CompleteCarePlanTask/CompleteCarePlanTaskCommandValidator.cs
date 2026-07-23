using FluentValidation;

namespace HealthPlatform.Application.Wellness.CarePlans.CompleteCarePlanTask;

public sealed class CompleteCarePlanTaskCommandValidator : AbstractValidator<CompleteCarePlanTaskCommand>
{
    public CompleteCarePlanTaskCommandValidator()
    {
        RuleFor(command => command.CarePlanId).NotEmpty();
        RuleFor(command => command.TaskId).NotEmpty();
    }
}
