using FluentValidation;

namespace HealthPlatform.Application.NextOfKin.DeleteNextOfKinContact;

public sealed class DeleteNextOfKinContactCommandValidator : AbstractValidator<DeleteNextOfKinContactCommand>
{
    public DeleteNextOfKinContactCommandValidator()
    {
        RuleFor(command => command.ContactId)
            .NotEmpty();
    }
}
