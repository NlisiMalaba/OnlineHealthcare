using FluentValidation;

namespace HealthPlatform.Application.HealthRecords.DeleteHealthRecordEntry;

public sealed class DeleteHealthRecordEntryCommandValidator : AbstractValidator<DeleteHealthRecordEntryCommand>
{
    public DeleteHealthRecordEntryCommandValidator()
    {
        RuleFor(command => command.EntryId)
            .NotEmpty()
            .MaximumLength(32);
    }
}
