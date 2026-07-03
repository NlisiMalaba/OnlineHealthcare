using FluentValidation;
using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Application.HealthRecords.GrantHealthRecordAccess;

public sealed class GrantHealthRecordAccessCommandValidator : AbstractValidator<GrantHealthRecordAccessCommand>
{
    public GrantHealthRecordAccessCommandValidator()
    {
        RuleFor(command => command.DoctorId)
            .NotEmpty();

        RuleFor(command => command.AccessType)
            .IsInEnum();

        When(command => command.AccessType == HealthRecordAccessType.Sections, () =>
        {
            RuleFor(command => command.Sections)
                .NotNull()
                .Must(sections => sections!.Count > 0)
                .WithMessage("At least one section is required for section-scoped access.");
        });
    }
}
