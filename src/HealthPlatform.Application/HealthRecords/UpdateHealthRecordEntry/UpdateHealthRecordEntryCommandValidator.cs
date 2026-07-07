using FluentValidation;

namespace HealthPlatform.Application.HealthRecords.UpdateHealthRecordEntry;

public sealed class UpdateHealthRecordEntryCommandValidator : AbstractValidator<UpdateHealthRecordEntryCommand>
{
    public UpdateHealthRecordEntryCommandValidator()
    {
        RuleFor(command => command.EntryId)
            .NotEmpty()
            .MaximumLength(32);
    }
}
