using FluentValidation;

namespace HealthPlatform.Application.Maternal.AntenatalRecords.CreateAntenatalRecord;

public sealed class CreateAntenatalRecordCommandValidator : AbstractValidator<CreateAntenatalRecordCommand>
{
    public CreateAntenatalRecordCommandValidator()
    {
        RuleFor(command => command.EstimatedDueDate)
            .NotEmpty();

        RuleFor(command => command.GestationalAgeWeeks)
            .InclusiveBetween(0, 42);

        RuleFor(command => command.ObstetricDoctorId)
            .NotEmpty();
    }
}
